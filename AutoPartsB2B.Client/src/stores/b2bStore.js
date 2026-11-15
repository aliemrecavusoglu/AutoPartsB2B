import { defineStore } from 'pinia'
import axios from 'axios'
import Swal from 'sweetalert2' // Profesyonel bildirim kütüphanesi
import router from '../router'
import * as signalR from '@microsoft/signalr'

export const useB2bStore = defineStore('b2b', {
    state: () => ({
        products: [],
        cart: [],
        loading: false,
        searchQuery: '',
        selectedCategory: '',
        // Token varsa true yap, yoksa false
        isAuthenticated: !!localStorage.getItem('token'),
        isDarkMode: localStorage.getItem('theme') === 'dark', // Dark mode durumu
        // Tarayıcı hafızasından kullanıcı id değerini alır
        userId: localStorage.getItem('userId') || null
    }),
    getters: {
        cartCount: (state) => state.cart.reduce((count, item) => count + item.quantity, 0),
        categories: (state) => [...new Set(state.products.map(p => p.category))],

        // İskonto ve Fiyat Hesaplamaları
        cartTotal: (state) => state.cart.reduce((total, item) => total + (item.price * item.quantity), 0),
        discountAmount: (state) => {
            const total = state.cart.reduce((t, item) => t + (item.price * item.quantity), 0);
            return total > 0 ? (total * 0.15) : 0; // %15 Bayi İndirimi
        },
        netTotal: (state) => {
            const total = state.cart.reduce((t, item) => t + (item.price * item.quantity), 0);
            return total - (total > 0 ? (total * 0.15) : 0);
        },
        filteredProducts: (state) => {
            return state.products.filter(p => {
                const searchLower = state.searchQuery.toLowerCase();
                const matchNameOrOem = p.name.toLowerCase().includes(searchLower) || p.oemCode.toLowerCase().includes(searchLower);
                const matchCategory = state.selectedCategory ? p.category === state.selectedCategory : true;
                return matchNameOrOem && matchCategory;
            });
        }
    },
    actions: {
        //login metodu
        async login(username, password) {
            try {
                const response = await axios.post('https://localhost:7165/api/Auth/login', {
                    username: username,
                    password: password,
                    fullName: "dummy", // model doğrulamasını geçmek için eklendi
                    role: "dummy"      // model doğrulamasını geçmek için eklendi
                });

                const token = response.data.token;
                const userId = response.data.userId;

                localStorage.setItem('token', token);
                localStorage.setItem('userId', userId);

                this.isAuthenticated = true;
                this.userId = userId;

                Swal.fire({
                    toast: true,
                    position: 'top-end',
                    icon: 'success',
                    title: 'Giriş Başarılı',
                    showConfirmButton: false,
                    timer: 2000
                });

                this.initializeSignalR();
                router.push('/');
            } catch (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Hata',
                    text: 'Geçersiz bayi kodu veya şifre.',
                    confirmButtonColor: '#dc3545'
                });
            }
        },

        // Formdan gelen bilgilerle backend tarafına kayıt isteği gönderir
        async register(username, password) {
            try {
                await axios.post('https://localhost:7165/api/Auth/register', {
                    username: username,
                    password: password,
                    fullName: username, // .NET tarafındaki zorunluluğu geçmek için
                    role: "Dealer"      // .NET tarafındaki zorunluluğu geçmek için
                });

                Swal.fire({
                    icon: 'success',
                    title: 'Kayıt başarılı',
                    text: 'Oluşturduğunuz bilgilerle giriş yapabilirsiniz.',
                    confirmButtonColor: '#198754'
                });

                router.push('/login');
            } catch (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Kayıt başarısız',
                    text: 'Bu bayi kodu sistemde zaten kayıtlı olabilir.',
                    confirmButtonColor: '#dc3545'
                });
                console.error("Kayıt Hatası:", error);
            }
        },

        //logout metodu
        logout() {
            // Tarayıcı hafızasından bilgileri temizler
            localStorage.removeItem('token');
            localStorage.removeItem('userId');

            // State (durum) değişkenlerini sıfırlar
            this.isAuthenticated = false;
            this.userId = null;

            // Axios'un istek başlıklarından eski token'ı siler
            delete axios.defaults.headers.common['Authorization'];

            // Giriş ekranına yönlendirir
            router.push('/login');
        },

        toggleTheme() {
            this.isDarkMode = !this.isDarkMode;
            const theme = this.isDarkMode ? 'dark' : 'light';
            localStorage.setItem('theme', theme);
            document.documentElement.setAttribute('data-bs-theme', theme); // Bootstrap 5 Dark Mode tetikleyicisi
        },
        initTheme() {
            if (this.isDarkMode) {
                document.documentElement.setAttribute('data-bs-theme', 'dark');
            }
        },

        //SignalR ile canlı stok takibi
        async initializeSignalR() {
            // Not: URL'deki port (7165) senin Backend API portunla aynı olmalı. Farklıysa düzelt.
            const connection = new signalR.HubConnectionBuilder()
                .withUrl('https://localhost:7165/stockHub')
                .withAutomaticReconnect()
                .build();

            // Backend'den gelen "ReceiveStockUpdate" mesajını sürekli dinle
            connection.on("ReceiveStockUpdate", (productId, newStock) => {
                console.log(`📡 SİGNALR'DAN VERİ GELDİ -> Ürün ID: ${productId}, Yeni Stok: ${newStock}`);

                // Veri tipi uyuşmazlığına (String/Int) karşı her iki durumu da kontrol et
                const product = this.products.find(p => p.id === productId || p.id === String(productId));
                if (product) {
                    product.stockQuantity = newStock; // Ekrandaki stoğu sayfa yenilenmeden GÜNCELLE!
                }
            });

            try {
                await connection.start();
                console.log("SignalR bağlantısı başarılı! Canlı stok takibi aktif.");
            } catch (err) {
                console.error("SignalR Bağlantı Hatası: ", err);
            }
        },
        async fetchProducts() {
            this.loading = true;
            try {
                const response = await axios.get('https://localhost:7165/api/Products');
                // Gelen verilere frontend tarafında OEM kodu ekliyoruz
                this.products = response.data.map(p => ({
                    ...p,
                    oemCode: `OEM-${p.id.toString().padStart(4, '0')}-${p.category.substring(0, 2).toUpperCase()}`
                }));
            } catch (error) {
                console.error("Hata:", error);
            } finally {
                this.loading = false;
            }
        },
        async fetchOrders(userId = 1) {
            try {
                const response = await axios.get(`https://localhost:7165/api/Orders/${userId}`);
                return response.data;
            } catch (error) {
                console.error("Siparişler çekilemedi", error);
                return [];
            }
        },
        addToCart(product) {
            const existingItem = this.cart.find(item => item.id === product.id);
            if (existingItem) {
                existingItem.quantity++;
            } else {
                this.cart.push({ ...product, quantity: 1 });
            }

            // Sağ alt köşeden çıkan kibar Toast bildirimi
            Swal.fire({
                toast: true,
                position: 'bottom-end',
                icon: 'success',
                title: `${product.name} sepete eklendi!`,
                showConfirmButton: false,
                timer: 2000,

            });
        },
        removeFromCart(productId) {
            this.cart = this.cart.filter(item => item.id !== productId);
        },
        async checkout() {
            if (this.cart.length === 0) return;
            try {
                const payload = {
                    userId: this.userId,
                    items: this.cart.map(item => ({ productId: item.id, quantity: item.quantity }))
                };
                await axios.post('https://localhost:7165/api/Orders', payload);

                // Başarılı sipariş animasyonu
                Swal.fire({
                    icon: 'success',
                    title: 'Siparişiniz Alındı!',
                    text: 'Parçalar hazırlanıyor, stoklar güncellendi.',

                    confirmButtonColor: '##ffc107'
                });

                this.cart = [];
                await this.fetchProducts();
            } catch (error) {
                // Hata veya yetersiz stok animasyonu
                Swal.fire({
                    icon: 'error',
                    title: 'Sipariş Başarısız!',
                    text: 'Seçtiğiniz ürünlerde yeterli stok bulunmuyor.',
                    confirmButtonColor: '#dc3545'
                });
            }
        }
    }
})