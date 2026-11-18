"""Builds settings-catalog.json: the single source of truth that maps every
control on the legacy Teknik Ayarlar page to its DealerSettings column.

Options were extracted from https://platform.b2bstore.com/DealerSystemSetting
(store1) on 2026-11-17. Column types come from analiz/sema.csv.
"""
import csv, json, sys

SEMA = sys.argv[1]
OUT = sys.argv[2]

types = {}
with open(SEMA, encoding="utf-8-sig") as f:
    for row in csv.reader(f, delimiter=";"):
        if row[0] == "DealerSettings":
            types[row[2]] = {"sqlType": row[3], "maxLength": int(row[4])}

def opts(s):
    out = []
    for part in s.split(" | "):
        v, l = part.split("=", 1)
        out.append({"value": v, "label": l})
    return out

YN = opts("1=Göster | 0=Gösterme")
USE = opts("1=Kullan | 0=Kullanma")
CURRENCIES = opts("-1=Cari Fiyat Tipine Göre Göster | 0=Değiştirme | 1=TL | 2=$ | 3=€ | 4=GBP | 5=SEK | 6=DKK | 7=CHD | 8=¥ | 9=SAR | 10=KWD | 11=AUD | 12=IRR | 13=BGN | 14=lei")

PRODUCT_DETAIL = "1=Ağırlık Göster | 2=Hacim Göster | 4=Marka Gösterme | 8=Kur Gösterme | 16=Normal Fiyat Gösterme | 32=Açılır Pencere Fiyatı Gösterme | 64=Gerçek fiyat gösterme | 128=Fiyat Gösterme | 256=Birim Gösterme | 512=Resim Gösterme | 1024=Yoldaki Stok Gösterme | 2048=Sevk Edilmeye Başlayan Sipariş Adedi Gösterme | 4096=Piyasa Fiyatı Gösterme | 8192=Ambalaj Tipi Gösterme | 16384=OEM Gösterme | 32768=Eşdeğer Kod Gösterme | 65536=Ürün Kodu Gösterme | 131072=Ürün Kısa Kodu Gösterme | 262144=Ürün Kısa Açıklaması Gösterme | 524288=Varyasyon Gösterme | 1048576=Kdv Oranı Gösterme | 2097152=Komisyon Oranı Gösterme | 4194304=İndirim Gösterme | 8388608=Üretici Kodu Gösterme | 16777216=Minimum Sipariş Göster | 33554432=Ürünlerde Küçük Resim Gösterme | 67108864=Ürün Stok Sayfası Gösterme | 134217728=Sepete ekle gösterme | 268435456=Sayfalama Özelliği Kullanma | 536870912=Uyumlu Araçları Gösterme | 1073741824=Üretim Tarihi Göster | 2147483648=Önceki sonraki ürün butonu göster | 4294967296=KDV Hariç/Dahil Fiyatı Gösterme | 8589934592=Mevsim Gösterme | 17179869184=Lastik Etiketi Göster | 34359738368=Taksit Bilgilerini Göster | 68719476736=Alternatif Ürünleri Göster | 137438953472=İlgili Ürünleri Göster | 274877906944=Sipariş Geçmişini Göster | 549755813888=Yorumları Göster | 1099511627776=Icon 1 | 2199023255552=Icon 2 | 4398046511104=Icon 3 | 8796093022208=Icon 4 | 17592186044416=Icon 5 | 35184372088832=Icon 6 | 70368744177664=Akıllı Aramada Seçenek | 140737488355328=Ürün Talebi Göster | 281474976710656=Stok Gösterme | 562949953421312=Aynı Markaya Ait Ürünleri Göster | 1125899906842624=Birime Göre Sepete Ekle Göster | 2251799813685248=Menşei Göster | 4503599627370496=\"Hemen Al\" Butonu Göster | 18014398509481984=Alış Fiyatı Göster | 36028797018963968=Ek Alan 1 | 72057594037927936=Ek Alan 2 | 144115188075855872=Ek Alan 3 | 288230376151711744=Ek Alan 4 | 576460752303423488=Ek Alan 5 | 1152921504606846976=Ek Alan 6 | 2305843009213693952=Yoldaki Stok Not Göster | 4611686018427387904=Temsilci indirim yetkisi kullan"

PRODUCT_DETAIL2 = "1=Sabit fiyat göster | 2=\"Detay butonu\" gösterme | 4=Özel ikonları gizle | 8=Adet girişi gösterme | 16=Ürün Karşılaştırma | 32=Varyasyonda Fiyat Göster | 64=Stok yoksa sepete ekle alanı gözükmesin, stok açıklaması gözüksün | 128=Yakınlaştırma kullanma | 256=Kredi kartı fiyatı göster | 512=Ürün detay görselleri düz görünüm | 1024=Fiyat Kodu Göster | 2048=Havale Fiyatı Göster | 4096=Ürün Detay Görselleri Açık Olarak Gelsin | 8192=Barkod Numarası Göster | 16384=Video göster | 32768=Piyasa fiyatı açılır pencere göster | 65536=Fiyat açılır pencere göster | 131072=Ürünler dışa aktarım | 262144=Marka görseli gösterme | 524288=İlgili ürünleri filtreye göre getir | 1048576=İlgili ürünleri ürün grubuna (ProductRelatedCode) göre getir | 2097152=Ürün detayda filtre gösterme | 4194304=Ürün aramada varyasyon görsellerini göster | 8388608=Whatsapp ile sipariş ver göster | 16777216=Varyasyon tablosunda değerleri ters çevir | 33554432=Varyasyon tablosunda Ek Alan 1 göster | 67108864=Varyasyon tablosunda Ek Alan 2 göster | 134217728=Varyasyon tablosunda fiyat göster | 268435456=Oturum kapalıyken stok göster (İptal) | 536870912=Temsilcilere özel fiyat alanını kapat | 1073741824=Açılır pencerede ödeme metodlarının fiyatlarını göster | 2147483648=İlgili ürünleri filtreye göre toplu sepete ekleme | 4294967296=Uyumlu araçları açık olarak göster | 8589934592=Yeni sekmede aç göster | 17179869184=Ürüne tıklayınca ürün detaya gitmesin | 34359738368=Ürün sıralaması seçimini gösterme | 68719476736=Ürün detayda kampanyalı ürünleri göster | 137438953472=Ürün detayda sosyal medyada paylaş göster | 274877906944=Palet göster | 549755813888=Varyasyonda resim göster | 1099511627776=Ürün notu göster | 2199023255552=Ürünlerde sepete ekleye tıklayınca ürün detaya gitsin | 4398046511104=Kredi kartı taksitli fiyatı göster | 8796093022208=Paket içi kdv hariç/dahil adet fiyatı göster | 17592186044416=Kdv dahil/hariç fiyatı kdv oranına göre renkli göster | 35184372088832=Kredi kartı ile sonradan ödeme fiyatı göster | 70368744177664=Paket içi adet fiyatı göster | 140737488355328=Kredi kartı ile sonradan ödeme fiyatı 2 göster | 281474976710656=Çift aramada ön/arka lastik göster | 562949953421312=Gerçek stok göster | 1125899906842624=Rezerve stok göster | 2251799813685248=Cari fiyatı göster | 4503599627370496=İndirimli fiyatta normal fiyatın üstünü çiz | 9007199254740992=Açılır pencerede sabit ödeme metodlarının fiyatlarını göster | 18014398509481984=Sipariş mailinde piyasa fiyatı göster | 36028797018963968=Patlak resim göster"

GROUPS = [
    ("genel", "Genel", "Site durumu, bayi bilgileri ve ana sayfa davranışı"),
    ("tasarim", "Görünüm & Tasarım", "Logo, renkler, tema ve sayfa yerleşimi"),
    ("menu", "Menü Gösterimi", "B2B portalında hangi menülerin görüneceği"),
    ("urun-gosterim", "Ürün Listeleme & Stok", "Ürün listesinde fiyat, stok ve sıralama gösterimi"),
    ("urun-detay", "Ürün Detay", "Ürün kartı ve detay sayfasında gösterilecek alanlar"),
    ("arama", "Arama & Filtre", "Ürün arama davranışı ve filtreler"),
    ("sepet", "Sepet & Sipariş", "Sepet limitleri ve sipariş adımları"),
    ("odeme", "Ödeme & Cari Hesap", "Online ödeme, komisyon ve cari hesap ekranları"),
    ("fiyat", "Fiyat & Döviz", "Kur, para birimi ve indirim ayarları"),
    ("basvuru", "Başvuru Formu & Giriş", "Yeni bayi başvuru formu alanları ve giriş ekranı"),
    ("entegrasyon", "Entegrasyon & Takip Kodları", "ERP entegrasyonu, şifreleme ve analitik kodları"),
    ("admin", "Yönetim (Admin)", "Yönetim paneli ve gelişmiş iş kuralları"),
]

S = []  # settings

def add(column, group, label, control, *, options=None, description=None,
        legacy=None, depends=None, invert=False, placeholder=None, min=None):
    t = types[column]
    item = {
        "key": column,
        "group": group,
        "label": label,
        "control": control,
        "sqlType": t["sqlType"],
        "maxLength": t["maxLength"],
        "bitwise": control == "bitmask",
    }
    if options is not None: item["options"] = options
    if description: item["description"] = description
    if legacy: item["legacyControlId"] = legacy
    if depends: item["dependsOn"] = depends
    if invert: item["invert"] = True
    if placeholder: item["placeholder"] = placeholder
    if min is not None: item["min"] = min
    S.append(item)

# ---------------- Genel
add("SiteStatus", "genel", "Site durumu", "select", options=opts("1=Aktif | 0=Pasif"),
    description="Pasif yapıldığında B2B portalına bayi girişi kapanır.", legacy="ddlSiteStatus")
add("DealerName", "genel", "Bayi adı", "text", legacy="TxtDealerName")
add("DealerPhoneNumber", "genel", "Bayi telefonu", "text", legacy="txtBayiTelefon")
add("DealerAdress", "genel", "Bayi adresi", "textarea", legacy="txtBayiAdres")
add("DealerWebAdress", "genel", "Bayi web adresi", "text", placeholder="https://", legacy="txtBayiWebAdres")
add("EInvoiceNo", "genel", "E-Fatura numarası", "text", legacy="txtEInvoiceNo")
add("HomeStyle", "genel", "Giriş sonrası açılacak sayfa", "select",
    options=opts("0=Varsayılan | 1=Ürünler | 2=E-Ödeme"), legacy="ddlHomeStyle")
add("AllProductsShow", "genel", "\"Tüm ürünler\" bağlantısını göster", "switch", legacy="ddlAllProductsShow")
add("SiteMap", "genel", "Site haritasını göster", "switch", legacy="ddlSiteMap")

# ---------------- Tasarım
add("Logo", "tasarim", "Logo (koyu)", "image", legacy="imgLogo")
add("Logo2", "tasarim", "Logo (açık)", "image", legacy="imgLogo2")
add("LogoLogin", "tasarim", "Giriş sayfası logosu", "image", legacy="imgLogoLogin")
add("Favicon", "tasarim", "Favicon", "image", legacy="imgFavicon")
add("NoImageImage", "tasarim", "Resimsiz ürünlerde varsayılan görsel", "image", legacy="imgNoImage")
add("LayoutColor", "tasarim", "Ana renk", "color", legacy="txtLayoutColor")
add("LayoutColor2", "tasarim", "İkincil renk", "color", legacy="txtLayoutColor2")
add("ParameterTheme", "tasarim", "Tema", "select",
    options=opts("0=Standart Tasarım | -1=Otomotiv Tasarım") + [{"value": str(i), "label": f"Tasarım {i}"} for i in range(1, 31)],
    legacy="ddParameterThema")
add("Layout", "tasarim", "Ürün listesinde \"Sepete Ekle\" butonunun yeri", "select",
    options=opts("1=Sağda | 0=Solda"), legacy="ddlLayout")
add("ProductImageAspectRatio", "tasarim", "Ürün görseli en/boy oranı", "text", placeholder="ör. 1:1", legacy="txtProductImageAspectRatio")
add("CategoryMenuVisible", "tasarim", "Kategori menüsü gösterimi", "select",
    options=opts("1=Menü Gösterimi | 2=Anasayfa Resimli Gösterim | 3=Resimsiz Gösterim | 4=Tüm Sayfada Resimli Gösterim | 0=Gösterme"),
    legacy="ddlCategoryMenu")
add("DontShowMenuAtDealers", "tasarim", "Bayilerde menüyü göster", "switch",
    description="Kolon adı \"DontShow\" olsa da eski ekranda 1 = Göster anlamında kullanılıyor.", legacy="ddlDontShowMenuAtDealers")
add("ParameterDesign", "tasarim", "Tasarım parametreleri", "bitmask", options=opts(
    "1=Ürün Tablo Görünümünde Görseli Doğrudan Göster | 2=Ürün Varsayılan Görünümü Resimli Görünüm | 4=Ürün Varsayılan Görünümü Uzun Resimli Görünüm | 8=İkonlu Menü Gösterimi | 16=Renkli İkonlu Menü Gösterimi | 32=Normal Menü Gösterimi | 64=Kategori Menüsünde Resim Göster | 128=Oval Görünüm | 256=Ürünlerde Sol Kategori Menüsü Göster | 512=Tam ekran | 1024=Günün Fırsatları Tekli Banner | 2048='Marka Menü' Üstte Göster | 4096=Ürün resmine tıklanınca büyük resim gözüksün | 8192=Mobilde özel tasarım | 16384=Top menü göster | 32768=Sabit B2B Menüsü | 65536=Sol Kategori Menüsü Göster V2 | 131072=\"Header\" Menü Göster | 262144=Anasayfa Ürünlerini Banner Olarak Göster | 524288=\"Kategori Menü\" Solda Göster | 1048576=\"Kategori Menü\" Üstte Göster | 2097152=\"B2B Menü\" Gösterme | 4194304=\"B2B Giriş\" Göster | 8388608=\"Bayi Menü\" Gösterme | 16777216=\"Yeni Üyelik\" Pop Up | 33554432=\"Login'de Header Menü\" Göster | 67108864=\"Login'de Footer Menü\" Göster | 134217728=Mobilde Menü Kapalı Gelsin | 268435456=Header menü koyu arka plan | 536870912=Anasayfada kategori banner göster | 1073741824=Anasayfada marka banner göster | 2147483648=Dinamik Arama Göster | 4294967296=Normal Menü Gösterimi (Koyu Arkaplan) | 8589934592=Mobilde Masaüstü Menü Tasarımı | 17179869184=Kategori Menü Üstte Göster V2 | 34359738368=İkinci menü gösterme | 68719476736=Detaylı arama göster | 137438953472=Filtreyi Göster | 274877906944=Kategoriden bağımsız bütün filtreleri göster | 549755813888=Sadece ürüne bağlı Kategorileri göster | 1099511627776=Enerjik Giriş | 2199023255552=Ürün Varsayılan Görünümü Çoklu Görünüm | 4398046511104=Ürün blok görünümde ürün adında satır sınırı kaldır | 8796093022208=Ana sayfa ürünlerinde başlık gösterme | 17592186044416=Anasayfa kategori menüsünde alt kategorilerin hepsi birlikte görünsün | 35184372088832=Ürün Varsayılan Görünümü Tablo Görünümü | 70368744177664=Dinamik arama üstte göster | 140737488355328=Hesabım menüsüne tıkladığında sabit b2b menü açılmasın | 281474976710656=Akordiyon Kategori | 562949953421312=İndirim kapatma switchi göster | 1125899906842624=Sesle arama göster | 2251799813685248=Kategori üst menü koyu arka plan | 4503599627370496=Mobilde tekli ürün görünümü | 9007199254740992=Sağ tık ve kopyalama devre dışı | 18014398509481984=Stok kapatma switchi göster | 36028797018963968=Kategori menü üstte gösterimde alt menü geniş gelsin | 72057594037927936=Site arama motorlarında taranmasın | 144115188075855872=Tek dil kullanan site url'lerinde dil kodu görünmesin"),
    legacy="ddlParameterDesign")

# ---------------- Menü
add("ParameterMenu", "menu", "Menü gösterimi", "bitmask", options=opts(
    "1=Siparişleri Gösterme | 2=Mesajları Gösterme | 4=Bakiye Siparişleri Gösterme | 8=Ödeme Sistemlerini Gösterme | 16=Sepet Menüsünü Gösterme | 32=Ürün Stok Sayfasını Gösterme | 64=Çok Görüntülenenleri Gösterme | 128=Yeni Ürünleri Gösterme | 256=Stok Açıklamalarını Gösterme | 512=Sıralamayı Gösterme | 1024=Stokta Olanlar Menüsünü Gösterme | 2048=Ürünler Menüsünü Gösterme | 4096=Cari Hesap Hareketleri Menüsünü Gösterme | 8192=Hesap Bakiyesi Menüsünü Gösterme | 16384=Valör Takip Menüsünü Gösterme | 32768=Hesap Numaraları Menüsünü Gösterme | 65536=Haberler Menüsünü Gösterme | 131072=Hızlı Sipariş Menüsünü Gösterme | 262144=Online Ödeme Menüsünü Gösterme | 524288=Kredi Kartı Ödemeleri Menüsünü Gösterme | 1048576=Alt bayi Menüsünü Gösterme | 2097152=Teklif Menüsünü Gösterme | 4194304=Teklif Listesi Menüsünü Gösterme | 8388608=Dövizler Menüsünü Gösterme | 16777216=Talepler Menüsünü Gösterme | 33554432=Kategori Seçimini Gösterme | 67108864=Marka Seçimini Gösterme | 134217728=Sezon Seçimini Gösterme | 268435456=Günün Fırsatları Aramasını Gösterme | 536870912=Çok Satanlar Aramasını Gösterme | 1073741824=Kampanya Aramasını Gösterme | 2147483648=Yeni Ürünler Aramasını Gösterme | 4294967296=Çok Görüntülenenler Aramasını Gösterme | 8589934592=Promosyon butonu göster | 17179869184=Adres Eklemede Fatura Adresi Seçeneğini Göster | 34359738368=Giriş Menüsünü Gösterme | 68719476736=Aranan Kelime Kaydedilsin | 137438953472=Adres Eklerken TC Kimlik No Göster | 274877906944=Kampanya Butonu Göster | 549755813888=Yeni ürünler butonu göster | 1099511627776=Adres eklerken ülke seçimi göster | 2199023255552=Genel arama gösterme | 4398046511104=Vadeli fiyat seçimi göster | 8796093022208=Oem Araması Göster | 17592186044416=Girişte kvkk onayı iste | 35184372088832=WhatsApp destek göster | 70368744177664=Run flat filtresi göster | 140737488355328=Yorumlar Menüsünü Gösterme | 281474976710656=Cari Kod Gösterme | 562949953421312=Barkod ile ürün arama | 1125899906842624=Sadece stoktakiler seçili gelsin | 2251799813685248=Markalar menüsü göster | 4503599627370496=Firma Sayfası Göster | 9007199254740992=Hesabımı sil menüsü göster | 18014398509481984=Bildirimler menüsü göster | 36028797018963968=Borç/alacak ay bazında göster | 72057594037927936=Vadesi Geçen ay bazında göster | 144115188075855872=Barkodla sepete ürün ekle göster | 288230376151711744=Perakende fiyat seçimi göster | 576460752303423488=Müşteri seçiminde fiyat tipi görünsün | 1152921504606846976=Haftanın ürünleri menüsünü göster | 2305843009213693952=Adres güncelle butonu gösterme | 4611686018427387904=Akıllı arama gösterme"),
    description="Dikkat: \"... Gösterme\" ile biten seçenekler işaretlendiğinde ilgili menü GİZLENİR.",
    legacy="ddlParameterMenu")

# ---------------- Ürün listeleme & stok
add("DoNotShowPrice", "urun-gosterim", "Fiyat gösterimi kısıtlaması", "select", options=opts(
    "0=Kullanım Dışı | 1=B2C'de Fiyatları Üye Girişi Olmadan Gösterme | 2=B2B'de Fiyatları Gösterme | 4=Fiyatları Sıfır Olan Ürünleri Listeleme | 5=B2C'de üye girişi olmadan sadece piyasa fiyatını göster"),
    legacy="ddlDontShowPrice")
add("HasStockVisible", "urun-gosterim", "Stok takibi", "select", options=opts(
    "0=Stok takibi yapma, tüm ürünleri stokta var göster | 1=Stok takibi yap, stokta olmayana sipariş verme | 2=Stok takibi yap, stokta olmayana sipariş ver | 3=Stok takibi yap, stoktan fazlası eklenirse uyarıp onay iste | 4=Ürün kartındaki \"Stok Takibi\" ayarı geçerli olsun | 5=Stok miktarı kadar sipariş verilebilir"),
    legacy="ddlHasStock")
add("ProductTableStockType", "urun-gosterim", "Stok gösterim tipi", "select", options=opts(
    "0=Adet Katları | 1=Var / Yok | 7=Var / Yok (İkon) | 2=Adet (Sayısal) | 5=4'e Kadar Adet Göster | 4=20'ye Kadar Adet Göster | 6=100'e Kadar Adet Göster | 3=Gösterme"),
    description="Tek seçimli alan: değer doğrudan kolona yazılır.", legacy="ddlProductTableStockType")
add("ParameterStock", "urun-gosterim", "Stok / depo gösterim seçenekleri", "bitmask", options=opts(
    "1=Depoları göster | 2=Depoları mouse ile stok değerinin üzerine gelince göster | 4=Müşteriler depoları görmesin, yalnız müşteri temsilcileri görsün | 8=Müşteriler Tüm Stokları Görsün | 16=Stok Olmasa Bile Depoları Göster | 64=Stok 0'sa Stok Sorunuz Gözüksün | 128=Login olmadan stok gösterme | 256=Müşteri temsilcisi stokların hepsini görmesin | 512=Stoktan bağımsız ürünleri sorunuz göster | 1024=Depolar sepete ekleye tıklayınca gelsin"),
    depends={"key": "ProductTableStockType", "notIn": ["3"], "message": "Stok gösterim tipi \"Gösterme\" iken bu seçeneklerin çoğu etkisizdir."},
    legacy="ddlParameterStock")
add("TaxInRatio", "urun-gosterim", "KDV gösterimi", "select", options=opts(
    "0=Tüm fiyatları KDV hariç göster | 1=Tüm fiyatları KDV dahil göster | 2=Liste fiyatı KDV hariç, diğerleri KDV dahil | 3=B2C'de tüm fiyatları KDV hariç göster"),
    legacy="ddlTaxInRatio")
add("ProductSort", "urun-gosterim", "Varsayılan ürün sıralaması", "select", options=opts(
    "0=Normal | 1=Ürün No | 2=Ürün adı | 3=Ürün kodu (A → Z) | 8=Ürün kodu (Z → A) | 4=Stok | 5=Marka | 6=Özel Sıralama | 7=Kampanya"),
    legacy="ddlProductSort")
add("MainProducts", "urun-gosterim", "Ana sayfada gösterilecek ürün blokları", "bitmask", options=opts(
    "64=Anasayfa | 128=Kampanya | 256=En Çok Satılan | 512=Yeni ürün | 1024=Çok Görüntülenen | 2048=Günün Ürünü | 4096=Haftanın Ürünü | 8192=Ayın Ürünü | 16384=Markalar | 1=Part 1 | 2=Part 2 | 4=Part 3 | 8=Part 4 | 16=Part 5 | 32=Part 6"),
    legacy="ddMainProducts")
add("Schedule", "urun-gosterim", "Yeni / çok satan / anasayfa ürünlerini manuel seçeceğim", "bitmask",
    options=opts("1=Yeni, çok satan ve anasayfada gösterilecek ürünleri manuel seçeceğim"),
    description="Kapalıysa bu listeler sistem tarafından otomatik oluşturulur.", legacy="ddlSchedule")
add("VariationShow", "urun-gosterim", "Varyasyon gösterimi", "select", options=opts(
    "0=Seçilmedi | 1=Tablolu (2 seçenekli varyasyon) | 2=Tablolu (çok seçenekli varyasyon) | 3=Açılır liste | 4=Çok seçimli (toplam adet alanına yansır) | 8=Tek seçimli | 16=Stok bazlı kutucuklu"),
    legacy="ddlVariationShow")
add("ExportProductList", "urun-gosterim", "Dışa aktarım butonları", "bitmask", options=opts(
    "1=Ürünleri Excel'e Aktar | 2=Sepeti PDF'e Aktar | 4=Cari Hareketleri Excel'e Aktar | 8=Sepeti Excel'e Aktar | 16=Sipariş takipte Excel'e aktar | 32=Bakiye siparişte Excel'e aktar | 64=Cari hareketlerde PDF'e aktar | 128=Excel aktarımda ürün görseli göster | 256=Bakiye siparişi PDF'e aktar"),
    legacy="ddlExportProductList")
add("TwoWorkingSystem", "urun-gosterim", "B2B & B2C ortak çalışma", "select", options=opts("1=Hayır | 0=Evet"),
    description="Eski ekranda değerler ters: 1 = Hayır, 0 = Evet. Kolon eşleşmesi kontrol edilecek.", legacy="ddlCommonSystem")
add("MinStockFilter", "urun-gosterim", "Minimum stok adedi filtresi", "number", min=0, legacy="txtMinStockFilter")

# ---------------- Ürün detay
add("ParameterProductDetail", "urun-detay", "Ürün detay parametreleri", "bitmask", options=opts(PRODUCT_DETAIL), legacy="ddlProductDetailParameter")
add("ParameterProductDetail2", "urun-detay", "Ürün detay parametreleri 2", "bitmask", options=opts(PRODUCT_DETAIL2), legacy="ddlProductDetailParameter2")
add("ParameterProductDetailNotLogin", "urun-detay", "Ürün detay parametreleri (B2C – oturum kapalı)", "bitmask", options=opts(PRODUCT_DETAIL),
    description="Üye girişi yapmamış ziyaretçilerin gördüğü ürün detayını belirler.", legacy="ddlParameterProductDetailNotLogin")

# ---------------- Arama & filtre
add("ParameterProductSearch", "arama", "Ürün arama parametreleri", "bitmask", options=opts(
    "1=Araç marka bölümünü aramada göster | 2=Müşteri Temsilcisi Tüm Fiyatları Görsün | 4=Müşteri Temsilcisi Alış Fiyatını Görsün | 8=Sadece tam eşleşen ürünleri bul | 16=Hızlı arama kullan | 32=Marka model aramasında tüm seçimler yapılırsa arama yapılsın | 64=Ürün aramada ürün kodu devre dışı | 128=Çift arama göster | 256=Kategori adına göre arama otomatik olmasın | 512=Ürün filtresi limiti olmasın | 1024=Ürün aramada ürün adı devre dışı | 2048=Ürün aramada marka adı devre dışı | 4096=Sadece tam eşleşen ürünleri bul (boşlukları silerek) | 8192=Şasi no ile arama göster | 16384=Kategori aramasında özel sıralama kullan | 32768=İlk listelemede ürünler gelmesin | 65536=Çift aramada grup sıralaması aktif | 131072=Hızlı aramada tam eşleşme devre dışı | 262144=Cari bazlı arama limiti kullan | 524288=Kategori url'si devre dışı | 1048576=Ürün kodunda tam eşleşme devre dışı"),
    legacy="ddlProductSearchParameter")
add("ParameterDynamicSearch", "arama", "Dinamik arama alanları", "bitmask", options=opts("1=Genel Arama | 2=Marka | 4=Modeli"),
    legacy="ddlParameterDynamicSearch")
add("SearchBrandShow", "arama", "Markaya göre arama", "switch", legacy="ddlSearchMarka")
add("SearchShowDetail", "arama", "Detaylı arama", "switch", legacy="ddlSearchDetail")
add("DontUseLcnAutoComplate", "arama", "Hızlı aramada listeleme kullanma", "switch", legacy="chkDontUseLcnAutoComplate")
add("SortFilterShow", "arama", "Filtre panelini göster", "switch", legacy="ddlFilterSort")
add("UseFiltering", "arama", "Gösterilecek filtreler", "bitmask", options=opts(
    "2=Marka Filtresi | 4=Ürün Özellikleri | 8=Varyasyon Filtresi | 16=Kategori Filtresi"),
    description="Bitwise değer, metin (nvarchar) kolonda sayı olarak saklanıyor.",
    depends={"key": "SortFilterShow", "in": ["1"], "message": "Filtre paneli kapalıyken bu seçimler portalda görünmez."},
    legacy="ddUseFiltering")

# ---------------- Sepet & sipariş
add("MinCartCount", "sepet", "Minimum sepet adedi", "number", min=0, legacy="txtMinCartCount")
add("MinCartTotal", "sepet", "Minimum sepet tutarı", "number", min=0, legacy="txtMinCartTotal")
add("MaximumOrderQuantity", "sepet", "Ürün başına maksimum sipariş adedi", "number", min=0,
    description="0 = sınırsız.", legacy="txtMaximumOrderQuantity")
add("ParameterOrder", "sepet", "Alışveriş sepeti parametreleri", "bitmask", options=opts(
    "1=Siparişte Sözleşme Onayı İste | 2=Siparişte Teslimat Adresi Seçimi İste | 4=Siparişte Kargo Seçimi İste | 8=Siparişte Fatura Adresi Seçimi İste | 16=Sipariş ekranında liste fiyatı gözükmesin | 32=Kupon Göster | 64=Bakiye Siparişte Teslimat Tarihini Gösterme | 128=Siparişte Ek Alan-2 Gösterilsin | 256=Siparişte Ürün Kısa Açıklama Gösterilsin | 512=Sepette İndirim Gösterme | 1024=Yeni Adres Ekle Gösterme | 2048=Container Kullan | 4096=Sepette Promosyon Ürünleri Göster | 8192=Sepet Ürünlerini Kategorilere Göre Grupla | 16384=Sipariş mailinde sadece indirimli fiyat göster | 32768=Kapıda ödemeyi ödeme seçenekleri içinde göster | 65536=Sepette fiyatları kdv dahil göster | 131072=Mailde hesap bakiyesi göster | 262144=Siparişte google analytics kullan | 524288=Vade Farkı Gönderme | 1048576=Sepete ekleyen göster | 2097152=Cari Limit Takibi | 4194304=Cari Limit Uyarısı | 8388608=Bekleyen siparişler limit takibine dahil edilsin | 16777216=Fatura ve teslimat adreslerini ayır | 33554432=Bakiye siparişte fiyat göster | 67108864=Marka bazlı iskonto kullan | 134217728=EFatura basarken dbs ve limit kontrolu yap | 268435456=Sepete eklenmiş mi? Kontrolü yapılsın | 536870912=Sepete eklerken bakiye sipariş kontrolü yap | 1073741824=Sepette ürün toplamları KDV dahil | 2147483648=Vade tarihi gösterme | 4294967296=Sepette döviz toplamı göster | 8589934592=Sepette adet toplamları ürün çeşidine göre olsun | 17179869184=Cari hareketlerde açıklama gösterme | 34359738368=Sipariş mailinde siparişi tamamlayan göster | 68719476736=Sipariş mailinde döviz kurlarını göster | 137438953472=Marka veya Ödeme Türüne Göre Sepeti Canlı Güncelle | 274877906944=Siparişte gönderilen sipariş göster | 549755813888=Koli Bazlı Sipariş / Koli Adet Gösterimi | 1099511627776=Variasyon grubu bazında sepete ekleme | 2199023255552=Sepette pazar yeri kodu göster | 4398046511104=Bayi tarafı sipariş detayında fiyatlar kdv dahil gözüksün | 8796093022208=Cari hesaplarda ciro göster | 17592186044416=Sepette ana ürünleri göster | 35184372088832=Sepette iade sözleşmesi iste | 70368744177664=Bakiye siparişte iptal talebi göster | 140737488355328=Sipariş detayda kalan göster | 281474976710656=Sepette kredi kartı ödemelerini tek çekim ve taksit olarak ayır | 562949953421312=Siparişte Firma Etiket Yorumu Görünsün | 1125899906842624=Temsilci İçin İç Not Alanı Göster | 2251799813685248=Eski Temsilci Satış Raporu Sekmelerini Göster | 4503599627370496=Cari hareketlerde işlem tipleri göster | 9007199254740992=Cari hareketlerde gerçekleşen vade göster | 18014398509481984=Desi bazlı kargo kullan | 36028797018963968=Sepeti sıfırla göster | 72057594037927936=Sepet ürünlerini kendi kurlarıyla göster | 144115188075855872=Müşteri Temsilcisi indirim girdiğinde cari indirimini yok saysın"),
    legacy="ddlOrderParameter")
add("PaymentDetailShow", "sepet", "Sepette ödeme detayını göster", "switch", legacy="ddlPaymentDetailShow")
add("CartProductVariationCodeShow", "sepet", "Sepette ürün varyasyon kodunu göster", "switch", legacy="ddlCartProductVariationCodeShow")
add("DeliveryDateShow", "sepet", "Siparişte teslimat tarihi al", "switch", legacy="chkDeliveryDateShow")
add("DeliveryAddress", "sepet", "\"Yeni teslimat adresi ekle\" seçeneğini göster", "switch", legacy="DDLDeliveryAddress")
add("SendDeliveryAddressIfDifferent", "sepet", "Fatura ve teslimat adresi aynıysa teslimat adresini boş kaydet", "switch", legacy="ddlSendDeliveryAddressIfDifferent")
add("AddCartAction", "sepet", "\"Ekle\" butonuna tıklanınca ürün detayını aç", "switch", legacy="chkAddCartAction")
add("DoNotSendMailToCustomer", "sepet", "Müşteriye sipariş maili gönderilmesin", "switch", legacy="chkDoNotSendMailToCustomer")
add("BillingAdressControl", "sepet", "Adres eklerken firma bilgileri eklensin", "switch", legacy="chkBillingAdress")

# ---------------- Ödeme & cari
add("ParameterPayment", "odeme", "Ödeme parametreleri", "bitmask", options=opts(
    "1=Ödemede sözleşme onayı iste | 2=E-Ödeme'de komisyon dahil | 4=E-Ödeme'de komisyon seçimli | 8=Vadesi geçmiş müşteriler ödemede taksit yapamasın | 32=Online ödemede kur seçimi | 64=Ödemede Pos Para Birimi Kullan | 128=Online ödemede vadesi geçen borcu var ise taksit gösterme | 256=Online ödemede Pos iskonto oranını gösterme | 512=Online ödemede vadesi geçen göster | 1024=Ödeme tiplerinde indirim gösterme | 2048=Kredi kartı ödemelerini tek çekim ve taksit olarak ayır | 4096=BankBinV2 yapısı aktif et | 8192=Ödeme detayda komisyon göster | 16384=Ödeme detayda hesaba geçen net tutar göster | 32768=Firma Bazlı Ödeme Grubu Kullansın"),
    legacy="ddlPaymentParameter")
add("CommissionType", "odeme", "E-Ödeme komisyonu", "select", options=opts(
    "0=Seçilmedi | 1=Sipariş verildiğinde komisyon farkı ürünlere yansıtılsın | 3=Normal gösterim"),
    legacy="ddlCommissionType")
add("BankTableType", "odeme", "Banka taksit gösterimi", "select", options=opts("1=Normal Gösterim | 0=Tablolu Gösterim"), legacy="ddlBankTableType")
add("PaymentLink", "odeme", "Farklı ödeme linki", "text", placeholder="https://", legacy="txtPaymentLink")
add("PaymentForAccountActivityShow", "odeme", "Cari hesaptan ödeme", "switch", legacy="ddlPaymentForAccountActivityShow")
add("LimitTracing", "odeme", "Cari limit takibi yapılsın", "switch", legacy="ddlCariLimitTracing")
add("LimitTracingInfo", "odeme", "Cari limit uyarısı göster", "switch",
    depends={"key": "LimitTracing", "in": ["1"], "message": "Cari limit takibi kapalıyken uyarı gösterilmez."},
    legacy="ddlCariLimitTracingInfo")
add("ExpiryDate", "odeme", "Caride vade tarihini göster", "switch", legacy="ddlExpiryDate")
add("AccountActivityGenralCuurencyTableShow", "odeme", "Caride toplam bakiye tablosunu göster", "switch", legacy="ddlAccountActivityGenralCuurencyTableShow")
add("RowDescriptionShow", "odeme", "Cari hesap hareketlerinde satır detayı", "switch", legacy="ddlRowDescriptionShow")
add("ParameterAccount", "odeme", "Hesap (bakiye) parametreleri", "bitmask", options=opts(
    "1=Limit gösterme | 2=Toplam Bakiye Sipariş Gösterme | 4=Toplam Borç Gösterme | 8=Toplam Alacak Gösterme | 16=Limit Kalan Bakiye Gösterme | 32=Diğer Bakiye Gösterme | 64=Hesap Bakiyesinde Toplam Siparişi Gösterme | 128=Vadesi geçen gösterme | 256=Fiyat Tipine Göre Hesap Numarası Göster | 512=Üst menüde bakiye gösterme"),
    legacy="ddlParameterAccount")
add("BonusShow", "odeme", "Puan göster", "switch", legacy="ddlBonusShow")

# ---------------- Fiyat & döviz
add("UseCurrencyType", "fiyat", "Döviz kuru", "select", options=opts("1=Otomatik | 0=Elle ayarla"), legacy="ddlCurrencyType")
add("CurrencyPriceType", "fiyat", "Döviz kuru fiyatı", "select", options=opts("1=Efektif satıştan alsın | 2=Döviz satıştan alsın"), legacy="ddlCurrencyPriceType")
add("CurrencyProduct", "fiyat", "Ürün para birimi", "select", options=CURRENCIES, legacy="ddlProductCurrency")
add("CurrencyPos", "fiyat", "POS para birimi", "select", options=CURRENCIES, legacy="ddlPosCurrency")
add("IntegrationCurrencyID", "fiyat", "Sipariş entegrasyon aktarımında döviz tipi", "select",
    options=[{"value": "0", "label": "Seçilmedi"}] + [o for o in CURRENCIES if o["value"] not in ("-1", "0")], legacy="ddIntegrationCurrency")
add("DiscountType", "fiyat", "İndirim tipi", "select", options=opts("A=Normal | P=Ürüne göre | C=Firmaya göre"),
    description="Veritabanında A / P / C olarak saklanıyor (eski ekranın HTML'inde seçenek değerleri tırnaklı: 'A').", legacy="ddlDiscountType")
add("DiscountTypeInvoice", "fiyat", "Fatura indirim tipi", "select", options=opts("0=Tutar | 1=Oran"), legacy="ddldiscountTypeInvoice")
add("SpecialDiscount", "fiyat", "Müşteri temsilcisi indirim yetkisi", "select",
    options=opts("0=Gösterme | 1=1 | 2=2 | 3=Müşteri temsilcisi bazında belirle"), legacy="ddlSpecialDiscount")

# ---------------- Başvuru formu & giriş
add("ParameterLogin", "basvuru", "Başvuru formu alanları", "bitmask", options=opts(
    "1=Adı | 2=Soyadı | 4=Firma | 8=Telefon | 16=Vergi dairesi | 32=Vergi numarası | 64=İl | 128=İlçe | 256=Adres | 512=Email | 1024=Bayi Web Adresi | 2048=TC Kimlik No | 4096=Başvuruda Sözleşme Onayı | 8192=Yeni Üyelik Yazısı | 16384=Üyelik butonu gizle | 32768=Üyeliksiz Alışverişi Göster | 65536=Alt bayi oluştururken menü ayarları gözükmesin | 131072=Posta Kodu | 262144=Açıklama | 524288=Şifre | 1048576=Bireysel | 2097152=Adres eklemede teslimat ve fatura adresi seçimi göster | 4194304=Dosya yükle göster | 8388608=Mobil telefon | 16777216=Üye olan firmanın promosyon kullanımı aktif olsun | 33554432=Üye olan firmanın stok gösterimi kapalı olsun | 67108864=Üyelikte aktivasyon linki oluştur | 134217728=Üye olan firmanın sipariş tamamlama butonu kapalı olsun | 268435456=Üye olan firmanın fiyatı kapalı olsun | 536870912=Üye olan firmanın piyasa fiyatı kapalı olsun"),
    legacy="ddlParameterLogin")
add("ParameterLoginMenu", "basvuru", "Başvuru formu menü gösterimi", "bitmask", options=opts(
    "1=Adı | 2=Soyadı | 4=Firma | 8=Telefon | 16=Vergi dairesi | 32=Vergi numarası | 64=İl | 128=İlçe | 256=Adres | 512=Email | 1024=Bayi Web Adresi | 2048=TC Kimlik No | 4096=Başvuruda Sözleşme Onayı | 8192=Vergi Onay Kodu | 16384=Üyelik butonu gizle | 32768=Bireysel-kurumsal seçimi | 65536=Telefon numarası | 131072=Adres | 262144=Açıklama"),
    legacy="ddlParameterLoginMenu")
add("RegisterFormUrl", "basvuru", "Harici kayıt formu linki", "text", placeholder="https://",
    description="Doluysa \"Yeni üyelik\" butonu bu adrese yönlendirir.", legacy="txtRegisterFormUrl")
add("UseCustomerCodeForLogin", "basvuru", "Girişte müşteri kodu iste", "switch", legacy="chkCustomerCodeForLogin")
add("LoginWithExpiry", "basvuru", "İlgili vade ile giriş yap", "switch", legacy="chkLoginWithExpiry")

# ---------------- Entegrasyon
add("Integrated", "entegrasyon", "ERP entegrasyonu var", "switch", legacy="ddlintegrated")
add("ParameterIntegration", "entegrasyon", "Entegrasyon parametreleri", "bitmask", options=opts(
    "1=Çapraz iskonto manuel tanımlanacak | 2=Müşteri grubu manuel belirlenecek | 4=Sipariş aktarımında alt bayi siparişini ana bayi olarak gönder | 8=Cari aktarımında otomatik etiket güncelle"),
    legacy="ddlParameterIntegration")
add("UseDesktopApi", "entegrasyon", "Masaüstü uygulaması kullan", "switch", legacy="ddlUseDesktopApi")
add("UseMd5Encryption", "entegrasyon", "Şifreleme", "select", options=opts("0=Normal | 1=MD5"),
    description="Akınsoft veya Vue (yeni yapı) kullananlar MD5, eski Dealer yapısı Normal seçmeli.", legacy="ddlUseMd5Encryption")
add("DealerAutHorizePersonel", "entegrasyon", "reCAPTCHA kodu", "text",
    description="Eski ekranda \"Recaptcha Kodu\" etiketiyle bu kolona (DealerAutHorizePersonel – yetkili kişi) yazılıyor.", legacy="txtBayiYetkiliKisi")
add("GoogleAdWordsReMarketingCode", "entegrasyon", "Takip kodu – NoScript", "textarea", legacy="txtGoogleAdWordsReMarketingCode")
add("GoogleAnalysticScript", "entegrasyon", "Takip kodu – Meta", "textarea", legacy="txtGoogleAnalysticScript")
add("GoogleAnalysticProfileID", "entegrasyon", "Takip kodu – Script", "textarea", legacy="txtGoogleAnalysticProfileID")

# ---------------- Admin
add("ParameterAdmin", "admin", "Admin parametreleri", "bitmask", options=opts(
    "1=Teknik Ayarlarda Teklif Modülü Göster | 2=Fiyat Tipinde KDV Muaf Göster | 4=Pos Yönetiminde Taksit Limiti Göster | 8=Firma Bazlı Gizleme | 16=Pos Yönetiminde Firma Bazlı Taksit Oranları Göster | 32=Sipariş Excel'e Aktar Butonunu Göster | 64=Bakiye Siparişi Excel'e Aktar Butonunu Göster | 128=Sipariş Detay Pdf'e aktar Butonunu göster | 256=Sepet'de Cariye tıklandığında Hesap Bakiyesinin Gösterilmesi | 512=Cari Haraketler Detay PDF'e aktar Butonunu göster | 1024=Bölge Bazlı Depo Gösterimi | 2048=SQL Veri Yapısı seçeneğini göster | 4096=Xml Dışa Aktar: Güncelleme Saati seçeneği | 8192=Raporlarda hizmet siparişlerini göster | 16384=Müşteri Temsilcisi Seçimi (Kullanıcılar) | 32768=Müşteri Temsilcisi Seçimi (Müşteri Temsilcileri) | 65536=Firma Ekleme: Gösterilecek Firma Parametreleri Seçimi | 131072=Ödeme Parametreleri: Vadesi geçmiş müşteriler ödemede taksit yapamasın | 262144=Alt Bayi Parametre | 524288=Firma bazında marka gizleme | 1048576=Ürün Gizle | 2097152=Stok 0'sa \"Tedarik Edilebilir\" Olarak Göster | 4194304=Depo Gizleme | 8388608=Firma: Cari Takip Dövizi Gösterimi | 16777216=Sepette Kategori Göster | 33554432=Dışa Aktarım Json | 67108864=Hesap limit göster | 134217728=Tahsilat girişi göster | 268435456=Siparişte müşteri temsilcisi kodu gönder | 536870912=Ürün Bazında Kampanya Gösterme | 1073741824=Çapraz iskontoda ürün iskonto 2 kullan | 2147483648=Firma bazında marka iskonto yapısı | 4294967296=Sipariş detayda iade butonu | 8589934592=Müşteri Temsilcisi Yalnız Cari Depoları Görsün | 17179869184=Depoya göre sepete ekle | 34359738368=Depo bazında sepete ekle göster | 68719476736=Gelişmiş Çapraz İskonto (Ödeme Tipine Göre) | 137438953472=Cari Bazlı Banner Popup Gösterimi | 274877906944=Entegrasyonda Kampanyalı Fiyat Güncelle | 549755813888=Ek alan 2'yi sadece müşteri temsilcisi görsün | 1099511627776=B4B Tek fiyat tipli varsayılan ürün gösterimi | 2199023255552=B4B Birden fazla fiyat tipli varsayılan ürün gösterimi | 4398046511104=Platform > Ürünler > İlgili Ürünler Gösterilsin | 8796093022208=Teknik ayarlarda indirim tipi gösterilsin | 17592186044416=Tüm carilerin ürün sipariş geçmişini temsilcilere göster | 35184372088832=Vade Gün Taksit Kısıtlama | 70368744177664=Vadesi geçen borcu olan sipariş gönderemesin | 140737488355328=Girişi Bayi Girişi ve Üye Girişi olarak ayır | 281474976710656=Fatura Görseli Göster (GİB) | 562949953421312=Cari Hareketlerde DBS işlemleri göster | 1125899906842624=Online ödemede hesap bakiyesi göster | 2251799813685248=Sipariş onaylandığında otomatik cari hareketlere eklensin | 4503599627370496=Ön sipariş kullan | 9007199254740992=Sipariş tamamla devre dışı | 18014398509481984=Sepette ürün arama göster | 36028797018963968=Standart Depo Bölme | 72057594037927936=Gelişmiş Depo Bölme | 144115188075855872=Bölge bazlı banner resmi gösterimi | 288230376151711744=Ürün detayda seo etiketi kullan | 576460752303423488=Temsilci girişini kapat | 1152921504606846976=Online ödeme temsilci için hep açık olsun | 2305843009213693952=Avrupa için Fatura Tasarımı | 4611686018427387904=Periyotlu siparişte başlangıç ve bitiş tarihi gözüksün"),
    legacy="ddlParameterAdmin")
add("SubDealerShow", "admin", "Bayi – alt bayi özelliği", "switch", legacy="ddSubDealerShow")
add("ParameterModules", "admin", "Parametrik modüller", "bitmask", options=[],
    description="Eski ekranda bu listenin hiç seçeneği yok.", legacy="ddlParameterModules")

# ------------------------------------------------------------------ checks
MAX = {"tinyint": 2**8 - 1, "smallint": 2**15 - 1, "int": 2**31 - 1, "bigint": 2**63 - 1}
warnings = []
for s in S:
    for o in s.get("options", []):
        o["value"] = o["value"].strip()
    if s["bitwise"]:
        if s["sqlType"] in MAX:
            limit = MAX[s["sqlType"]]
            for o in s["options"]:
                if int(o["value"]) > limit:
                    o["unsupported"] = True
                    warnings.append(f'{s["key"]} ({s["sqlType"]}): "{o["label"]}" = {o["value"]} kolona sığmıyor')
        # duplicates
        vals = [o["value"] for o in s["options"]]
        assert len(vals) == len(set(vals)), s["key"]
        for v in vals:
            n = int(v)
            assert n & (n - 1) == 0, (s["key"], v)
        s["knownMask"] = str(sum(int(v) for v in vals if not any(o.get("unsupported") and o["value"] == v for o in s["options"])))

keys = [s["key"] for s in S]
assert len(keys) == len(set(keys)), "duplicate key"

catalog = {
    "version": 1,
    "source": "https://platform.b2bstore.com/DealerSystemSetting (store1, 2026-11-17)",
    "groups": [{"key": k, "title": t, "description": d} for k, t, d in GROUPS],
    "settings": S,
    "notInDatabase": [
        {"legacyControlId": "ddlParameterProductDetailNotLogin2", "label": "Ürün Detay Parametresi 2 (B2C - Oturum Kapalı)"},
        {"legacyControlId": "ddlParameterMenu2", "label": "Menü Gösterimi 2"},
        {"legacyControlId": "chkWhatsAppOrderNotification", "label": "WhatsApp Sipariş Bildirimi Gönder"},
        {"legacyControlId": "FavoriteIconPosition", "label": "Favori İkonu Pozisyonu (Sol/Sağ, Üst/Alt)"},
    ],
}
with open(OUT, "w", encoding="utf-8") as f:
    json.dump(catalog, f, ensure_ascii=False, indent=2)

print(f"{len(S)} ayar, {sum(s['bitwise'] for s in S)} bitwise")
for w in warnings: print("UYARI:", w)
