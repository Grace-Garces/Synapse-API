// /JS/api.js

const API_URL = 'http://localhost:5292'; // URL da sua API

// Helper para criar cabeçalhos com o token
function getAuthHeaders() {
    const headers = { 'Content-Type': 'application/json' };
    const token = auth.getToken();
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    return headers;
}

// Central de API
const api = {
    // --- AUTH (Sem mudanças) ---
    registerUser: async (email, password) => {
        const response = await fetch(`${API_URL}/api/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });
        const data = await response.json();
        if (!response.ok) throw new Error(data.message || 'Erro ao criar conta');
        return data;
    },

    loginUser: async (email, password) => {
        const response = await fetch(`${API_URL}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });
        const data = await response.json();
        if (!response.ok) throw new Error(data.message || 'Credenciais inválidas');
        return data; // Retorna { token, email }
    },

    // --- PERSONA (Sem mudanças) ---
    generatePersona: async (description) => {
        const response = await fetch(`${API_URL}/api/persona/generate`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({ description })
        });
        if (response.status === 401) auth.logout();
        if (!response.ok) throw new Error('Falha ao gerar persona');
        return response.json();
    },

    // --- COLEÇÕES (NOVOS ENDPOINTS) ---

    // NOVO: Busca as coleções do usuário (para a Home)
    getCollections: async () => {
        const response = await fetch(`${API_URL}/api/collections`, {
            method: 'GET',
            headers: getAuthHeaders()
        });
        if (response.status === 401) auth.logout();
        if (!response.ok) throw new Error('Falha ao buscar coleções');
        return response.json();
    },

    // --- FUNÇÃO ADICIONADA AQUI ---
    // NOVO: Busca detalhes de UMA coleção
    getCollectionDetails: async (collectionId) => {
        const response = await fetch(`${API_URL}/api/collections/${collectionId}`, {
            method: 'GET',
            headers: getAuthHeaders()
        });
        if (response.status === 401) auth.logout();
        if (response.status === 404) throw new Error('Coleção não encontrada (404).');
        if (!response.ok) throw new Error('Falha ao buscar detalhes da coleção.');
        return response.json();
    },
    // --- FIM DA FUNÇÃO ADICIONADA ---

    // NOVO: Cria a definição da coleção
    createCollection: async (name, systemContext) => {
        const response = await fetch(`${API_URL}/api/collections`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({ name, systemContext })
        });
        if (response.status === 401) auth.logout();
        if (!response.ok) throw new Error('Falha ao criar coleção');
        return response.json(); // Retorna { id, name }
    },

    // ATUALIZADO: Faz upload de arquivos para uma coleção específica
    uploadFiles: async (collectionId, files) => {
        const formData = new FormData();
        for (const file of files) {
            formData.append('files', file);
        }

        const response = await fetch(`${API_URL}/api/collections/${collectionId}/upload`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${auth.getToken()}`
                // Não defina Content-Type, o navegador faz isso
            },
            body: formData
        });

        if (response.status === 401) auth.logout();
        const responseText = await response.text();
        if (!response.ok) throw new Error(responseText);
        return responseText;
    },

    // ATUALIZADO: Conversa com uma coleção específica
    queryCollection: async (collectionId, question) => {
        const response = await fetch(`${API_URL}/api/collections/${collectionId}/ask`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({ question: question }) // A API só precisa da pergunta
        });
        
        if (response.status === 401) auth.logout();
        if (!response.ok) throw new Error('Falha ao consultar');
        return response.json();
    }
};

// Expondo globalmente
window.api = api;