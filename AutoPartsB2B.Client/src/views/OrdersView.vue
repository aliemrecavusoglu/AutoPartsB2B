<template>
    <div class="container mt-4">
        <h2 class="mb-4 text-dark fw-bold border-start border-warning border-5 ps-3">📦 Geçmiş Siparişlerim</h2>

        <div v-if="loading" class="text-center my-5">
            <div class="spinner-border text-warning" role="status"></div>
        </div>

        <div v-else-if="orders.length === 0" class="alert alert-secondary border-0 shadow-sm">
            Henüz hiç sipariş vermediniz. B2B portalından parça ekleyerek başlayabilirsiniz.
        </div>

        <div v-else class="row g-4">
            <!-- İstatistik (chart) alanı -->
            <div class="col-lg-5 mb-3">
                <div class="card shadow-sm border-0 border-top border-warning border-4 bg-light h-100">
                    <div class="card-header bg-white py-3 border-0">
                        <h6 class="fw-bold text-secondary mb-0"><i class="bi bi-pie-chart-fill text-warning me-2"></i>
                            Ürün Bazlı Harcama Dağılımı</h6>
                    </div>
                    <div class="card-body d-flex justify-content-center align-items-center" style="min-height: 300px;">
                        <!-- Vue-Chartjs bileşeni -->
                        <Doughnut v-if="chartData.labels.length > 0" :data="chartData" :options="chartOptions" />
                    </div>
                </div>
            </div>

            <!-- Mevcut siparişler listesi -->
            <div class="col-lg-7">
                <div class="row">
                    <div class="col-12 mb-4" v-for="order in orders" :key="order.id">
                        <div class="card shadow-sm border-0 border-start border-success border-4 bg-light">
                            <div class="card-header bg-white d-flex justify-content-between align-items-center py-3">
                                <span class="fw-bold text-secondary">Sipariş No: #{{ order.id }}</span>

                                <div class="d-flex align-items-center gap-3">
                                    <span class="badge bg-success fs-5">{{ order.totalAmount.toFixed(2) }} ₺</span>
                                    <button @click="downloadInvoice(order.id)"
                                        class="btn btn-outline-danger btn-sm fw-bold shadow-sm">
                                        <i class="bi bi-file-earmark-pdf-fill"></i> İndir
                                    </button>
                                </div>
                            </div>
                            <div class="card-body">
                                <ul class="list-group list-group-flush border-0">
                                    <li class="list-group-item d-flex justify-content-between bg-transparent px-0"
                                        v-for="(item, index) in order.items" :key="index">
                                        <span class="fw-bold text-dark">{{ item.quantity }}x <span class="fw-normal">{{
                                            item.name }}</span></span>
                                        <span class="text-danger fw-bold">{{ item.subTotal.toFixed(2) }} ₺</span>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'; 
import { useB2bStore } from '../stores/b2bStore';
import axios from 'axios';
import Swal from 'sweetalert2';

// Chart.js gerekli kütüphaneleri
import { Doughnut } from 'vue-chartjs'
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js'

// Chart.js'i aktifleştir
ChartJS.register(ArcElement, Tooltip, Legend)

const store = useB2bStore();
const orders = ref([]);
const loading = ref(true);

onMounted(async () => {
    // Try-catch bloğu ile api hatalarını yakalıyoruz
    try {
        // Giriş yapan aktif kullanıcının id numarasını kullanıyoruz
        orders.value = await store.fetchOrders(store.userId);
    } catch (error) {
        console.error("Siparişler yüklenirken hata oluştu:", error);
    } finally {
        loading.value = false;
    }
});

// Sipariş verilerini analiz edip grafiğe uygun formata çeviren fonksiyon
const chartData = computed(() => {
    // Orders boşsa grafik objesini boş döndürerek çökmesi engellendi
    if (!orders.value || orders.value.length === 0) {
        return { labels: [], datasets: [] };
    }

    const productTotals = {};

    // Tüm siparişleri ve içindeki ürünleri gezerek toplam harcamaları bul
    orders.value.forEach(order => {
        if (!order.items) return;

        order.items.forEach(item => {
            if (!productTotals[item.name]) {
                productTotals[item.name] = 0;
            }
            productTotals[item.name] += item.subTotal;
        });
    });

    const labels = Object.keys(productTotals);
    const data = Object.values(productTotals);

    // Kurumsal renk paleti
    const backgroundColors = ['#ffc107', '#198754', '#dc3545', '#0dcaf0', '#6f42c1', '#fd7e14'];

    return {
        labels: labels,
        datasets: [
            {
                backgroundColor: backgroundColors.slice(0, labels.length),
                data: data,
                borderWidth: 2,
                hoverOffset: 4
            }
        ]
    }
});

// Grafik ayarları ve temaya uyumlu font renkleri
const chartOptions = computed(() => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: {
            position: 'bottom',
            labels: {
                // Karanlık moddaysa yazıları beyaz, değilse koyu gri yapar
                color: '#212529',
                font: { family: "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif", weight: 'bold' }
            }
        }
    }
}));

// Pdf indirme fonksiyonu
const downloadInvoice = async (orderId) => {
    try {
        // Pdf'i blob (dosya) formatında çekiyoruz
        const response = await axios.get(`https://localhost:7165/api/Orders/invoice/${orderId}`, {
            responseType: 'blob'
        });

        // Dosyayı tarayıcıda indirmeye zorlayan blob hilesi
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `Siparis_Faturasi_${orderId}.pdf`);
        document.body.appendChild(link);
        link.click();

        // Bellek temizliği
        link.parentNode.removeChild(link);
        window.URL.revokeObjectURL(url);

        Swal.fire({ toast: true, position: 'top-end', icon: 'success', title: 'Fatura İndirildi', showConfirmButton: false, timer: 1500 });

    } catch (error) {
        Swal.fire({ icon: 'error', title: 'Hata', text: 'Fatura indirilirken bir sorun oluştu.' });
        console.error("Pdf indirme hatası:", error);
    }
};
</script>