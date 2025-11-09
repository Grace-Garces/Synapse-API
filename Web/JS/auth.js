// /JS/auth.js
// Este é o código correto que define o objeto 'auth'

const auth = {
    saveToken: (token) => {
        // Salva o token no localStorage do navegador
        localStorage.setItem('brainapi_token', token);
    },

    getToken: () => {
        // Pega o token do localStorage
        return localStorage.getItem('brainapi_token');
    },

    isAuthenticated: () => {
        // Verifica se o token existe
        const token = auth.getToken();
        return !!token; // Retorna true se o token existir, false se não
    },

    logout: () => {
        // Remove o token e redireciona para o login
        localStorage.removeItem('brainapi_token');
        // Certifique-se que o caminho para sua página de login está correto
        window.location.href = '../WelcomePage/LoginPage.html';
    },

    redirectToLogin: () => {
        // Função de "guarda de rota"
        if (!auth.isAuthenticated()) {
            console.warn("Usuário não autenticado. Redirecionando para login.");
            window.location.href = '../WelcomePage/LoginPage.html';
        }
    }
};

// Torna o objeto 'auth' acessível globalmente (para outros scripts)
window.auth = auth;