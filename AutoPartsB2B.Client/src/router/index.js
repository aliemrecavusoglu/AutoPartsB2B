import { createRouter, createWebHistory } from 'vue-router'
import ProductsView from '../views/ProductsView.vue'
import OrdersView from '../views/OrdersView.vue'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/login',
            name: 'login',
            component: LoginView
        },
        {
            path: '/',
            name: 'products',
            component: ProductsView,
            meta: { requiresAuth: true }
        },
        {
            path: '/orders',
            name: 'orders',
            component: OrdersView,
            meta: { requiresAuth: true }
        },
        {
            path: '/register',
            name: 'register',
            component: RegisterView
        }
    ]
})

router.beforeEach((to, from, next) => {
    const isAuthenticated = !!localStorage.getItem('token');

    if (to.meta.requiresAuth && !isAuthenticated) {
        // Giriş yapmamış biri korumalı sayfaya girmeye çalışırsa login'e at
        next('/login');
    } else if (to.path === '/login' && isAuthenticated) {
        // Zaten giriş yapmış biri login'e gitmeye çalışırsa ana sayfaya at
        next('/');
    } else {
        next();
    }
})

export default router