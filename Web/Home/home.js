// /Home/home.js

document.addEventListener('DOMContentLoaded', () => {
    // "Guarda de Rota"
    auth.redirectToLogin();
    showLoader(true);

    // === ELEMENTOS DO PAINEL ===
    const sidebarList = document.getElementById('collection-list-ul');
    const btnLogout = document.getElementById('btn-logout');
    
    // Widget de Query Rápida
    const queryCollectionSelect = document.getElementById('query-collection-select');
    const queryQuestionInput = document.getElementById('query-question');
    const btnQuickQuery = document.getElementById('btn-quick-query');
    const queryOutputDiv = document.getElementById('quick-query-output');
    const queryAnswerP = document.getElementById('query-answer');

    // Widget de Estatísticas
    const statCollections = document.getElementById('stat-collections');
    const statDocuments = document.getElementById('stat-documents');
    const statChunks = document.getElementById('stat-chunks');
    const statQueries = document.getElementById('stat-queries');

    // Widget de Status
    const statusApi = document.getElementById('status-api');
    const statusOllama = document.getElementById('status-ollama');
    const statusModelChat = document.getElementById('status-model-chat');
    const statusModelEmbed = document.getElementById('status-model-embed');

    // Widget de Atividade
    const activityFeed = document.getElementById('activity-feed');

    // === EVENTOS ===
    if (btnLogout) {
        btnLogout.addEventListener('click', () => {
            if (confirm('Deseja sair?')) {
                auth.logout();
            }
        });
    }

    // Evento do Botão de Query Rápida
    btnQuickQuery.addEventListener('click', handleQuickQuery);

    // === FUNÇÕES DE CARREGAMENTO ===

    // 1. Carregar Coleções (Sidebar e Dropdown)
    async function loadCollections() {
        if (!sidebarList || !queryCollectionSelect) return;
        
        sidebarList.innerHTML = '<li style="padding: 1rem; color: var(--gray-500);">Carregando...</li>';
        queryCollectionSelect.innerHTML = '<option value="">Carregando...</option>';
        
        try {
            const collections = await api.getCollections();
            
            sidebarList.innerHTML = ''; 
            queryCollectionSelect.innerHTML = '<option value="">-- Selecione uma coleção --</option>';
            
            if (collections.length === 0) {
                sidebarList.innerHTML = '<li style="padding: 1rem; color: var(--gray-500);">Nenhuma coleção.</li>';
                queryCollectionSelect.innerHTML = '<option value="">Nenhuma coleção encontrada</option>';
                queryCollectionSelect.disabled = true;
                return;
            }

            // Popula a Sidebar e o Dropdown
            collections.forEach(collection => {
                const collectionUrl = `../Chat-Collection/chat-collection.html?id=${collection.id}`;

                // Adiciona à Sidebar
                const li = document.createElement('li');
                li.className = 'collection-item'; 
                li.innerHTML = `<a href="${collectionUrl}">${collection.name}</a>`;
                sidebarList.appendChild(li);

                // Adiciona ao Dropdown de Query Rápida
                const option = document.createElement('option');
                option.value = collection.id;
                option.textContent = collection.name;
                queryCollectionSelect.appendChild(option);
            });

        } catch (error) {
            sidebarList.innerHTML = `<li style="padding: 1rem; color: var(--error);">Erro ao carregar.</li>`;
            queryCollectionSelect.innerHTML = `<option value="">Erro ao carregar</option>`;
            console.error(error);
        }
    }

    // 2. Simular Verificação de Status (Front-end)
    function checkSystemStatus() {
        // Esta função simula chamadas a um futuro endpoint de "health check"
        // (Você pode criar um endpoint /api/health que retorna o status)
        
        // Simulação de API
        setTimeout(() => updateStatusIndicator(statusApi, true), 500);
        // Simulação de Ollama
        setTimeout(() => updateStatusIndicator(statusOllama, true), 800);
        // Simulação de Modelos
        setTimeout(() => updateStatusIndicator(statusModelChat, true), 1100);
        setTimeout(() => updateStatusIndicator(statusModelEmbed, true), 1400);
        
        // Exemplo de falha:
        // updateStatusIndicator(statusOllama, false, "Não encontrado");
    }

    function updateStatusIndicator(element, isOnline, message = "Online") {
        const dot = element.querySelector('.status-dot');
        
        element.childNodes[1].nodeValue = ` ${isOnline ? message : 'Offline'}`; // Atualiza o texto
        dot.className = `status-dot ${isOnline ? 'online' : 'offline'}`;
        
        if (!isOnline) {
            element.style.color = 'var(--error)';
        } else {
            element.style.color = 'var(--success)';
        }
    }

    // 3. Simular Carga de Estatísticas e Atividades
    function loadDashboardData() {
        // No futuro, você criaria endpoints como:
        // const stats = await api.getDashboardStats();
        // const activities = await api.getRecentActivity();

        // Simulação de Estatísticas
        statCollections.textContent = "3"; // (Vindo das coleções carregadas)
        statDocuments.textContent = "12";
        statChunks.textContent = "1482"; // Métrica RAG importante!
        statQueries.textContent = "78";

        // Simulação de Atividade
        activityFeed.innerHTML = `
            <li>
                <p>Documento <strong>"manual_v2.pdf"</strong> adicionado à coleção <strong>"Manuais Técnicos"</strong>.</p>
                <span class="time">há 15 minutos</span>
            </li>
            <li>
                <p>Nova coleção <strong>"Receitas da Vovó"</strong> criada.</p>
                <span class="time">há 1 hora</span>
            </li>
            <li>
                <p>Consulta na coleção <strong>"Manuais Técnicos"</strong>: "como resetar o modem?"</p>
                <span class="time">há 2 horas</span>
            </li>
        `;
    }

    // 4. Lidar com a Query Rápida
    async function handleQuickQuery() {
        const collectionId = queryCollectionSelect.value;
        const question = queryQuestionInput.value.trim();

        if (!collectionId) {
            alert('Por favor, selecione uma coleção.');
            return;
        }
        if (!question) {
            alert('Por favor, digite uma pergunta.');
            return;
        }

        showLoader(true, "Processando pergunta...");
        btnQuickQuery.disabled = true;

        try {
            const response = await api.queryCollection(collectionId, question);
            
            queryAnswerP.textContent = response.answer || "A API não retornou uma resposta.";
            queryOutputDiv.style.display = 'block';

        } catch (error) {
            queryAnswerP.textContent = `Erro: ${error.message}`;
            queryOutputDiv.style.display = 'block';
        } finally {
            showLoader(false);
            btnQuickQuery.disabled = false;
        }
    }

    // === INICIALIZAÇÃO ===
    async function initializeDashboard() {
        await loadCollections(); // Carrega coleções primeiro
        
        // Atualiza o stat de coleções com o número real
        const count = sidebarList.querySelectorAll('.collection-item').length;
        if (count > 0) {
            statCollections.textContent = count;
        }

        checkSystemStatus();
        loadDashboardData(); // Carrega os dados simulados
        showLoader(false);
    }

    initializeDashboard();
});

// Função global para mostrar loader
function showLoader(show, text = "Carregando...") {
    const loader = document.getElementById('loader');
    const loaderText = document.querySelector('.loader-text');
    if (loader) {
        loaderText.textContent = text;
        if (show) {
            loader.classList.add('active');
        } else {
            loader.classList.remove('active');
        }
    }
}