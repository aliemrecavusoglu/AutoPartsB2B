<template>
  <div id="app">
    <!-- Navbar -->
    <nav v-if="store.isAuthenticated" class="navbar navbar-expand-lg navbar-dark bg-dark shadow sticky-top">
      <div class="container">
        <a class="navbar-brand" href="#">🛠️ Kayseri Oto Yedek Parça B2B</a>

        <!-- Mobil görünümde sağ tarafta Sepet ve Hamburger menü yan yana dursun -->
        <div class="d-flex align-items-center order-lg-3 gap-2">
          <button class="btn btn-warning position-relative fw-bold text-dark btn-sm" type="button"
            data-bs-toggle="offcanvas" data-bs-target="#cartOffcanvas">
            <i class="bi bi-cart-fill"></i> Sepetim
            <span v-if="store.cartCount > 0"
              class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger border border-light">
              {{ store.cartCount }}
            </span>
          </button>

          <!-- Hamburger Menü İkonu -->
          <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav"
            data-bs-dismiss="offcanvas">
            <span class="navbar-toggler-icon"></span>
          </button>
        </div>

        <!-- Sayfa Linkleri -->
        <div class="collapse navbar-collapse order-lg-2" id="navbarNav">
          <ul class="navbar-nav ms-auto gap-2 mt-3 mt-lg-0 me-lg-3 align-items-center">

            <li class="nav-item me-2">
              <button @click="store.toggleTheme" class="btn btn-outline-light btn-sm border-0 fs-5"
                title="Temayı Değiştir">
                <i v-if="store.isDarkMode" class="bi bi-sun-fill text-warning"></i>
                <i v-else class="bi bi-moon-stars-fill text-light"></i>
              </button>
            </li>

            <li class="nav-item">
              <router-link to="/" class="btn btn-outline-light btn-sm w-100 fw-bold">🏠 Mağaza</router-link>
            </li>
            <li class="nav-item">
              <router-link to="/orders" class="btn btn-outline-warning btn-sm w-100 fw-bold">📦
                Siparişlerim</router-link>
            </li>
            <li class="nav-item ms-lg-2 mt-2 mt-lg-0">
              <button @click="store.logout" class="btn btn-danger btn-sm w-100 fw-bold"><i
                  class="bi bi-box-arrow-right"></i> Çıkış</button>
            </li>
          </ul>
        </div>
      </div>
    </nav>

    <div class="mt-4 pt-3">
      <!-- Yumuşak Sayfa Geçiş Animasyonu -->
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </div>

    <!-- Sağdan Açılan Sepet (Offcanvas) - Aynı Kaldı -->
    <div class="offcanvas offcanvas-end shadow" tabindex="-1" id="cartOffcanvas" style="top: 56px;">
      <div class="offcanvas-header bg-dark text-warning border-bottom border-warning border-3">
        <h5 class="offcanvas-title fw-bold">🛒 B2B Sepetim</h5>
        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="offcanvas"></button>
      </div>
      <div class="offcanvas-body d-flex flex-column bg-light">
        <div v-if="store.cart.length === 0" class="text-center text-muted mt-5">
          <span class="fs-1 d-block mb-3">🛠️</span>
          Sepetiniz şu an boş.
        </div>

        <ul class="list-group mb-3 shadow-sm" v-else>
          <li class="list-group-item d-flex justify-content-between align-items-center border-0 border-bottom"
            v-for="item in store.cart" :key="item.id">
            <div>
              <h6 class="my-0 fw-bold text-dark">{{ item.name }}</h6>
              <small class="text-muted">{{ item.quantity }} adet x {{ item.price }} ₺</small>
            </div>
            <div class="d-flex align-items-center">
              <span class="fw-bolder text-danger me-3">{{ item.price * item.quantity }} ₺</span>
              <button @click="store.removeFromCart(item.id)"
                class="btn btn-sm btn-outline-danger rounded-circle fw-bold"
                style="width: 30px; height: 30px; padding: 0;">X</button>
            </div>
          </li>
        </ul>

        <div class="mt-auto bg-white p-3 rounded shadow-sm border" v-if="store.cart.length > 0">
          <div class="d-flex justify-content-between fs-6 mb-1 text-muted">
            <span>Ara Toplam:</span>
            <span class="text-decoration-line-through">{{ store.cartTotal.toFixed(2) }} ₺</span>
          </div>
          <div class="d-flex justify-content-between fs-6 mb-2 text-success fw-bold">
            <span>%15 Bayi İskontosu:</span>
            <span>-{{ store.discountAmount.toFixed(2) }} ₺</span>
          </div>
          <hr class="my-2">
          <div class="d-flex justify-content-between fs-5 mb-3">
            <span class="fw-bold text-secondary">Net Ödenecek:</span>
            <span class="fw-bolder text-danger fs-4">{{ store.netTotal.toFixed(2) }} ₺</span>
          </div>
          <button @click="store.checkout"
            class="btn btn-warning btn-lg w-100 fw-bold text-dark text-uppercase shadow-sm" data-bs-dismiss="offcanvas">
            Siparişi Onayla <i class="bi bi-check-circle"></i>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useB2bStore } from './stores/b2bStore'

const store = useB2bStore()

onMounted(() => {
  store.initTheme(); // Sayfa yüklendiğinde seçili temayı (karanlık/aydınlık) uygula

  if (store.isAuthenticated) {
    store.initializeSignalR()
  }
})
</script>

<style>
/*Geçiş Animasyonu CSS Kodları*/
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.fade-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>