// /Create-Collection/create.js

document.addEventListener('DOMContentLoaded', () => {
    auth.redirectToLogin();

    // ===== ELEMENTOS =====
    const collectionName = document.getElementById('collection-name');
    const collectionDesc = document.getElementById('collection-desc');
    const contextBtns = document.querySelectorAll('.context-btn');
    const manualContextDiv = document.getElementById('manual-context');
    const aiContextDiv = document.getElementById('ai-context');
    const contextManual = document.getElementById('context-manual');
    const contextDescription = document.getElementById('context-description');
    const btnGenerateContext = document.getElementById('btn-generate-context');
    const btnAcceptContext = document.getElementById('btn-accept-context');
    const btnRegenerateContext = document.getElementById('btn-regenerate-context');
    const aiContextOutput = document.getElementById('ai-context-output');
    const generatedContext = document.getElementById('generated-context');
    const uploadArea = document.getElementById('upload-area');
    const fileInput = document.getElementById('file-input');
    const filesContainer = document.getElementById('files-container');
    const btnUpload = document.getElementById('btn-upload');
    const uploadStatus = document.getElementById('upload-status');
    const btnLogout = document.getElementById('btn-logout');
    const statusName = document.getElementById('status-name');
    const statusFiles = document.getElementById('status-files');
    const statusContext = document.getElementById('status-context');
    const menuToggle = document.getElementById('menu-toggle');

    let selectedFiles = [];
    let currentMode = 'manual';
    let contextMode = 'manual';

    // ===== FUNCTIONS =====

    // Switch Context Mode
    contextBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            contextBtns.forEach(b => {
                b.style.borderColor = 'transparent';
                b.style.background = 'transparent';
            });
            btn.style.borderColor = 'var(--primary)';
            btn.style.background = 'rgba(99, 102, 241, 0.2)';
            
            currentMode = btn.dataset.mode;
            
            if (currentMode === 'manual') {
                manualContextDiv.style.display = 'block';
                aiContextDiv.style.display = 'none';
                contextMode = 'manual';
                statusContext.textContent = '✓ Manual';
                statusContext.style.color = '#10b981';
            } else {
                manualContextDiv.style.display = 'none';
                aiContextDiv.style.display = 'block';
                contextMode = 'pending';
                statusContext.textContent = 'Gerando...';
                statusContext.style.color = '#f59e0b';
            }
        });
    });

    // Generate Context with AI
    btnGenerateContext.addEventListener('click', async () => {
        const description = contextDescription.value.trim();
        if (!description) {
            showStatusMessage('uploadStatus', 'Por favor, descreva o contexto desejado.', true);
            return;
        }

        showLoader(true);
        try {
            const data = await api.generatePersona(description);
            generatedContext.textContent = data.generatedContext;
            aiContextOutput.style.display = 'block';
            contextMode = 'ai';
            statusContext.textContent = '✓ Gerado';
            statusContext.style.color = '#10b981';
        } catch (error) {
            showStatusMessage('uploadStatus', 'Erro ao gerar contexto: ' + error.message, true);
        } finally {
            showLoader(false);
        }
    });

    // Accept AI Context
    btnAcceptContext.addEventListener('click', () => {
        contextManual.value = generatedContext.textContent;
        currentMode = 'manual';
        contextBtns[0].style.borderColor = 'var(--primary)';
        contextBtns[0].style.background = 'rgba(99, 102, 241, 0.2)';
        contextBtns[1].style.borderColor = 'transparent';
        contextBtns[1].style.background = 'transparent';
        manualContextDiv.style.display = 'block';
        aiContextDiv.style.display = 'none';
        showStatusMessage('uploadStatus', 'Contexto aceito! ✓', false);
    });

    // Regenerate Context
    btnRegenerateContext.addEventListener('click', () => {
        btnGenerateContext.click();
    });

    // Update Status
    function updateStatus() {
        statusName.textContent = collectionName.value || '-';
        statusFiles.textContent = selectedFiles.length + ' arquivo' + (selectedFiles.length !== 1 ? 's' : '');
    }

    // File Upload Handlers
    uploadArea.addEventListener('click', () => fileInput.click());

    uploadArea.addEventListener('dragover', (e) => {
        e.preventDefault();
        uploadArea.style.borderColor = 'var(--primary)';
        uploadArea.style.background = 'rgba(99, 102, 241, 0.15)';
    });

    uploadArea.addEventListener('dragleave', () => {
        uploadArea.style.borderColor = 'rgba(99, 102, 241, 0.3)';
        uploadArea.style.background = 'rgba(99, 102, 241, 0.1)';
    });

    uploadArea.addEventListener('drop', (e) => {
        e.preventDefault();
        uploadArea.style.borderColor = 'rgba(99, 102, 241, 0.3)';
        uploadArea.style.background = 'rgba(99, 102, 241, 0.1)';
        handleFiles(e.dataTransfer.files);
    });

    fileInput.addEventListener('change', (e) => {
        handleFiles(e.target.files);
    });

    function handleFiles(files) {
        selectedFiles = Array.from(files).slice(0, 5);
        renderFileList();
        updateStatus();
        
        if (selectedFiles.length > 0) {
            btnUpload.style.display = 'block';
        }
    }

    function renderFileList() {
        filesContainer.innerHTML = '';
        
        if (selectedFiles.length === 0) {
            filesContainer.innerHTML = '<p style="color: var(--gray-500); font-size: 0.85rem;">Nenhum arquivo selecionado</p>';
            return;
        }

        selectedFiles.forEach((file, index) => {
            const fileItem = document.createElement('div');
            fileItem.style.cssText = 'display: flex; justify-content: space-between; align-items: center; padding: 0.75rem; background: rgba(99, 102, 241, 0.1); border-radius: 6px; border: 1px solid rgba(99, 102, 241, 0.2);';
            
            const fileSize = (file.size / 1024 / 1024).toFixed(2);
            
            fileItem.innerHTML = `
                <div style="display: flex; align-items: center; gap: 0.75rem;">
                    <span style="font-size: 1.2rem;">📄</span>
                    <div>
                        <p style="margin: 0; color: var(--light); font-weight: 500; font-size: 0.9rem;">${file.name}</p>
                        <p style="margin: 0; color: var(--gray-500); font-size: 0.75rem;">${fileSize} MB</p>
                    </div>
                </div>
                <button class="btn-danger btn-small" onclick="removeFile(${index})">✕</button>
            `;
            
            filesContainer.appendChild(fileItem);
        });
    }

    // Remove File
    window.removeFile = (index) => {
        selectedFiles.splice(index, 1);
        renderFileList();
        updateStatus();
        
        if (selectedFiles.length === 0) {
            btnUpload.style.display = 'none';
        }
    };

    // Show Status Message
    function showStatusMessage(elementId, msg, isError = false) {
        const statusDiv = document.getElementById(elementId);
        const statusP = statusDiv.querySelector('p');
        statusP.textContent = msg;
        statusDiv.style.display = 'block';
        statusDiv.className = 'status ' + (isError ? 'error' : 'success');
    }

    // Upload Files
// Upload Files
    btnUpload.addEventListener('click', async () => {
        const name = collectionName.value.trim();
        const context = contextManual.value.trim();

        if (!name) {
            showStatusMessage('uploadStatus', 'Por favor, defina um nome para a coleção.', true);
            return;
        }

        if (selectedFiles.length === 0) {
            showStatusMessage('uploadStatus', 'Por favor, selecione pelo menos um arquivo.', true);
            return;
        }

        if (!context) {
            showStatusMessage('uploadStatus', 'Por favor, configure um contexto para a coleção.', true);
            return;
        }

        showLoader(true);
        let createdCollectionId = null; // Variável para guardar o ID

        try {
            // --- ETAPA 1: CRIAR A COLEÇÃO ---
            // Primeiro, chama a API para criar a definição da coleção
            showStatusMessage('uploadStatus', 'Criando definição da coleção...', false);
            const collectionData = await api.createCollection(name, context);
            
            if (!collectionData || !collectionData.id) {
                throw new Error("A API não retornou um ID de coleção válido.");
            }
            createdCollectionId = collectionData.id; // Salva o ID retornado
            
            // --- ETAPA 2: FAZER UPLOAD DOS ARQUIVOS ---
            // Agora, usa o ID para fazer o upload dos arquivos
            showStatusMessage('uploadStatus', 'Enviando arquivos...', false);
            const fileResponse = await api.uploadFiles(createdCollectionId, selectedFiles);

            showStatusMessage('uploadStatus', 'Coleção criada com sucesso! ✓', false);
            
            // Redireciona para a página de chat da nova coleção
            setTimeout(() => {
                // Modificado: Redireciona direto para a nova página de chat
                window.location.href = `../Chat-Collection/chat-collection.html?id=${createdCollectionId}`;
            }, 2000);

        } catch (error) {
            showStatusMessage('uploadStatus', 'Erro: ' + error.message, true);
        } finally {
            showLoader(false);
        }
    });
    btnLogout.addEventListener('click', () => {
        if (confirm('Deseja sair?')) {
            auth.logout();
        }
    });

    menuToggle.addEventListener('click', () => {
        // Implementar sidebar toggle se necessário
    });

    // Initial update
    updateStatus();
});

// Função global para mostrar loader
function showLoader(show) {
    const loader = document.getElementById('loader');
    if (loader) {
        if (show) {
            loader.classList.add('active');
        } else {
            loader.classList.remove('active');
        }
    }
}