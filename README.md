# 🛠️ Oto Yedek Parça B2B Portal

Modern, güvenli ve kullanıcı dostu bir B2B (Business-to-Business) otomotiv yedek parça sipariş ve bayi yönetim sistemi. Bu proje, bayilerin yedek parça stoklarını inceleyebileceği, güvenli sipariş verebileceği ve geçmiş harcamalarını istatistiksel olarak analiz edebileceği tam donanımlı bir full-stack çözümdür.

## 🚀 Öne Çıkan Özellikler

- **Kullanıcı Kimlik Doğrulama (Auth):** JWT (JSON Web Token) tabanlı güvenli bayi giriş ve kayıt sistemi.
- **Dinamik Tema (Dark/Light Mode):** Kullanıcı tercihine göre anında değişen, tam entegre karanlık ve aydınlık arayüz desteği.
- **Görsel İstatistikler (Dashboard):** Chart.js kullanılarak sipariş verilerinin JavaScript ile işlenmesi ve ürün bazlı harcama dağılımı grafikleri.
- **PDF Fatura Üretimi:** Backend üzerinden dinamik olarak oluşturulan sipariş faturalarının (PDF) frontend tarafında blob olarak indirilmesi.
- **Merkezi Durum Yönetimi:** Pinia ile sepet, tema ve kullanıcı oturum verilerinin sayfalar arası anlık senkronizasyonu.
- **Responsive Tasarım:** Bootstrap 5 ile mobil, tablet ve masaüstü cihazlara tam uyumlu modern arayüz.

## 💻 Kullanılan Teknolojiler

**Frontend:**
- Vue.js 3 (Composition API)
- Pinia (State Management)
- Bootstrap 5
- Chart.js & Vue-Chartjs
- Axios

**Backend & Veritabanı:**
- .NET 8 (C# Web API)
- Entity Framework Core
- JWT Bearer Authentication
- SQLite (Geliştirme ortamı konfigürasyonu)


## ⚙️ Kurulum ve Çalıştırma Adımları

Projeyi yerel ortamda çalıştırmak için aşağıdaki adımları izleyebilirsiniz.

**1. Backend (.NET 8 Web API) Kurulumu**
```bash
# Backend klasörüne girin (klasör adınız farklıysa ona göre güncelleyin)
cd AutoPartsB2B.sln

# Veritabanını oluşturun ve API'yi başlatın
dotnet ef database update
dotnet run
```

**2. Frontend (Vue 3) Kurulumu**
```bash
# Frontend klasörüne girin
cd AutoPartsB2B.Client

# Bağımlılıkları yükleyin ve geliştirme sunucusunu başlatın
npm install
npm run dev
```
