<template>
  <div class="container mt-4">
    <!-- Arama ve Filtreleme Alanı -->
    <div class="row mb-4">
      <div class="col-12">
        <!-- bg-white kaldırıldı, adaptif border eklendi -->
        <div class="card shadow-sm border border-2 border-secondary-subtle">
          <div class="card-body p-3 row g-3">
            <div class="col-md-8">
              <div class="input-group">
                <span class="input-group-text bg-transparent border-secondary-subtle border-end-0">
                  <i class="bi bi-search text-primary"></i>
                </span>
                <!-- form-control zaten temaya otomatik uyum sağlar -->
                <input v-model="store.searchQuery" type="text"
                  class="form-control form-control-lg border-secondary-subtle border-start-0 shadow-none"
                  placeholder="Parça adı veya OEM kodu ara...">
              </div>
            </div>
            <div class="col-md-4">
              <select v-model="store.selectedCategory"
                class="form-select form-select-lg border-secondary-subtle shadow-none">
                <option value="">Tüm Kategoriler</option>
                <option v-for="cat in store.categories" :key="cat" :value="cat">{{ cat }}</option>
              </select>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Ürün Kartları Listesi -->
    <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
      <div class="col" v-for="product in store.filteredProducts" :key="product.id">
        <!-- bg-white kaldırıldı, border-2 ve border-secondary-subtle eklendi -->
        <div class="card h-100 shadow-sm border border-2 border-secondary-subtle"
          style="border-bottom: 4px solid #ffc107 !important;">
          <div class="card-body d-flex flex-column">

            <h5 class="card-title fw-bold mb-1">{{ product.name }}</h5>

            <!-- text-muted yerine text-body-secondary kullanıldı (Karanlık moda tam uyumlu) -->
            <small class="text-body-secondary d-block mb-3">
              KOD: <span class="font-monospace">{{ product.oemCode }}</span>
            </small>

            <!-- Kategori yazısı için text-body-tertiary -->
            <p class="small text-body-tertiary text-uppercase fw-semibold mb-3">
              {{ product.category }}
            </p>

            <div class="mt-auto">
              <div class="d-flex justify-content-between align-items-center mb-3">
                <span class="badge bg-dark fs-6"><i class="bi bi-box-seam"></i> Stok: {{ product.stockQuantity }}</span>
                <span class="text-danger fw-bolder fs-4">{{ product.price.toFixed(2) }} ₺</span>
              </div>

              <button @click="store.addToCart(product)" class="btn btn-warning w-100 fw-bold text-darkshadow-sm">
                SEPETE EKLE <i class="bi bi-cart-plus"></i>
              </button>
            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useB2bStore } from '../stores/b2bStore'

const store = useB2bStore()

// Sayfa yüklendiğinde ürünleri Backend'den çek
onMounted(async () => {
  await store.fetchProducts()
})
</script>