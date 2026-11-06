# Project Overview
- Game Title: GD_HutanRockmoon (nama proyek; judul final belum ditentukan).
- High-Level Concept: Prototipe eksplorasi horor di hutan: pemain mengumpulkan tujuh dupa satu per perjalanan dan menaruhnya pada tujuh batu melingkar. Ritual lengkap menyalakan cahaya pusat dan memunculkan tirta yang dapat diambil.
- Players: Single player; hantu NPC roaming yang sudah ada dipertahankan.
- Inspiration / Reference Games: Belum ditentukan; mempertahankan suasana dan aset proyek saat ini.
- Tone / Art Direction: Hutan malam horor; aset sementara berbentuk primitive 3D, warna kayu/batu dan penanda cahaya lembut, mudah diganti tanpa mengubah logika.
- Target Platform: Standalone Windows 64-bit.
- Screen Orientation / Resolution: Landscape mengikuti Game View saat ini; HUD responsif, tidak mengganti resolusi proyek.
- Render Pipeline: URP, PC_RPAsset; Unity 6000.6.0f1; URP 17.6.0.

# Game Mechanics
## Core Gameplay Loop
1. Saat permainan dimulai, pilih lokasi valid secara acak untuk tiga dupa lepas, satu peti, dan satu pusat ritual.
2. Peti menyimpan tepat empat dupa. Pemain berinteraksi untuk mengambil satu dupa per penggunaan; tutup peti dapat memberi feedback sederhana saat pertama digunakan.
3. Pemain hanya membawa satu dupa. Jika tangan penuh, dupa/peti tidak berkurang dan pesan menjelaskan bahwa dupa perlu ditaruh dahulu.
4. Bawa dupa ke salah satu dari tujuh batu kosong dan gunakan interaksi untuk menaruhnya. Dupa yang ditaruh tidak dapat diambil kembali pada tahap ini.
5. Tujuh batu membentuk satu lingkaran di sekitar pusat yang acak: bukan tujuh lokasi bebas di seluruh map. Radius dan rotasi awal lingkaran dapat dikonfigurasi; ketinggian masing-masing batu menyesuaikan tanah.
6. Setelah tujuh batu terisi, cahaya pusat menyala dan tepat satu tirta muncul. Pemain dapat mengambilnya dengan E; tampilkan status bahwa tirta dimiliki.
7. Mekanisme serangan, mengusir, atau mengalahkan hantu dengan tirta **di luar cakupan**, sesuai pilihan pengguna.

Keputusan pengguna: satu dupa per perjalanan; randomisasi setiap permainan; tirta dapat diambil saja. Loop bolak-balik menambah paparan terhadap hantu dan ketegangan; cahaya dupa mengurangi kebuntuan pencarian tanpa penanda map penuh.

## Controls and Input Methods
- Pertahankan WASD, Shift, mouse look, Escape, serta perilaku lentera saat ini tanpa mengubah PlayerController.cs atau LanternController.cs.
- Input backend terverifikasi Both (activeInputHandler=2); Input System 1.20.1 terpasang. Interaksi baru menggunakan InputAction event-driven pada UnityEngine.InputSystem dengan binding E, enable/disable lifecycle yang benar.
- Kamera pemain menjadi sumber raycast, jarak awal 2.5 meter dapat diatur; dinding/objek solid memblokir interaksi. Abaikan collider pemain sendiri; collider dekorasi tidak boleh menghalangi targetnya sendiri secara tidak sengaja.
- E pada dupa: ambil jika tangan kosong. E pada peti: ambil satu jika tersedia dan tangan kosong. E pada batu kosong: tempatkan jika membawa dupa. E pada tirta: ambil setelah ritual lengkap.
- Jangan menerima interaksi saat cursor dilepas atau aplikasi kehilangan fokus; tidak menambah tombol serangan.

# UI
HUD minimal baru, disarankan uGUI karena belum ada Canvas/UIDocument runtime dan uGUI 2.6.0 tersedia. Implementasi UI mengikuti panduan uGUI yang perlu diperiksa sebelum pembuatan.

```text
+--------------------------------------------------+
| Ritual: 0/7                                      |
| Dibawa: kosong / 1 dupa                           |
|                                                  |
|                         ·                        |
|                [E] Ambil dupa                     |
|                                                  |
|          Pesan singkat: Tangan masih penuh        |
+--------------------------------------------------+
```
- Reticle kecil di pusat; petunjuk hanya muncul ketika target valid dalam jangkauan.
- Tampilkan peti kosong, batu sudah terisi, tangan penuh, kemajuan ritual, dan Tirta diperoleh.
- Teks Bahasa Indonesia, kontras tinggi, tidak menutupi lentera; prompt tidak menangkap klik atau mengganti cursor behavior pemain.
- HUD membaca state/event sistem; bukan sumber kebenaran jumlah dupa.

# Key Asset & Context
## State proyek yang diperiksa
- Scene gameplay: Assets/NatureStarterKit2/Scene/Hutan.unity; tersimpan tanpa perubahan saat pemeriksaan.
- Player dengan CharacterController dan PlayerController.cs; kamera Main Camera terhubung.
- GhostRoaming.cs menggunakan NavMeshAgent dan enam waypoint; GhostFloating.cs berada pada child model, bukan root agent.
- Terrain utama: Assets/NatureStarterKit2/Scene/New Terrain.asset, ukuran data 250×600×250, origin (-125,0,-125). Terrain sekunder lebih luas dan saling mencakup area; sampling ground harus memfilter terrain utama agar tidak salah memilih permukaan.
- Enam lampu yang ada: satu Directional intensity 0.25, dan lima Point intensity 0.1, 0.1, 1, 0.71, 4. Ambient Skybox intensity 0.5; bakedGI false. Tidak mengubah lampu, volume, atau ambient lama.
- Domain Reload untuk Play Mode nonaktif. Tidak mengubah setting ini tanpa persetujuan; pengujian manual dapat dilakukan jika pengujian otomatis tidak mendukung konfigurasi tersebut.

## Pendekatan implementasi
- Pilihan A, direkomendasikan: kandidat lokasi aman yang disiapkan dalam scene, lalu pilih kombinasi acak setiap permainan; validasi clearance tanah, kemiringan dan jalur dari pemain. Keuntungan: menghindari dupa tak terjangkau dan ring menembus pohon. Kekurangan: kandidat harus disiapkan terlebih dahulu. Tetap acak pada setiap permainan.
- Pilihan B: sampling bebas seluruh terrain secara runtime. Lebih banyak variasi, tetapi risiko di luar area playable, bertumpuk dengan vegetasi terrain, serta biaya validasi dan debugging lebih besar.
- Gunakan A untuk prototipe ini: jumlah kandidat lebih banyak daripada kebutuhan, berjauhan dan tersebar di area playable; satu kandidat ritual harus valid untuk seluruh tujuh batu serta pusat, bukan hanya titik tengah.

## Berkas/aset yang direncanakan
- Assets/Scripts/Interaction/IInteractable.cs: kontrak prompt, validasi dan interaksi.
- Assets/Scripts/Interaction/PlayerInteractor.cs: input E dan kamera raycast.
- Assets/Scripts/Ritual/IncenseInventory.cs: kapasitas satu, event perubahan, transaksi mengambil/menaruh.
- Assets/Scripts/Ritual/IncensePickup.cs: dupa lepas; state mencegah pengambilan ganda.
- Assets/Scripts/Ritual/IncenseChest.cs: empat unit, pengurangan stok hanya setelah transfer berhasil.
- Assets/Scripts/Ritual/RitualStone.cs: satu slot per batu, penanda dupa visual.
- Assets/Scripts/Ritual/RitualManager.cs: state tujuh slot, penyelesaian sekali, reward tirta.
- Assets/Scripts/Ritual/RitualSpawner.cs: random seed/config, memilih kandidat, membuat 3+4 dupa dan ring 7 batu.
- Assets/Scripts/Ritual/RitualSpawnAnchor.cs: kategori kandidat serta clearance yang bisa disesuaikan.
- Assets/Scripts/Ritual/TirtaPickup.cs: reward collectible tanpa logika combat.
- Assets/Scripts/UI/RitualHUD.cs: prompt/status/progress event-driven.
- Assets/Prefabs/Ritual/IncensePlaceholder.prefab: batang dupa, alas sederhana, collider interaksi, Visual child.
- Assets/Prefabs/Ritual/ChestPlaceholder.prefab: kotak kayu dengan tutup, stok 4.
- Assets/Prefabs/Ritual/RitualStonePlaceholder.prefab: batu primitive dengan placement socket.
- Assets/Prefabs/Ritual/TirtaPlaceholder.prefab: wadah/botol tirta sementara dengan collider.
- Assets/Prefabs/Ritual/RitualCenterPlaceholder.prefab: penanda pusat dan lampu awalnya mati.
- Assets/Materials/Ritual/: material URP untuk dupa, kayu, batu dan tirta, tanpa modifikasi material aset lama.
- Scene Hutan.unity: tambahkan root RitualSystem, spawn anchors, komponen interactor/inventory pada pemain dan HUD. Tidak mengubah penempatan lentera atau hantu.
- Assets/Tests/EditMode/Ritual/: tes state/inventory dan random layout; konfigurasi assembly tests bila diperlukan.
- Dependency/impact analysis wajib sebelum setiap edit aset yang sudah ada; bila GetDependency tidak tersedia, inspeksi dependency read-only AssetDatabase serta referensi serialized dan catat keterbatasan pemeriksaan dependents.

Prefab memisahkan Visual dari collider, socket dan script sehingga model final dapat mengganti child Visual tanpa mengubah gameplay. Aset dibuat dari primitive Unity, tidak memerlukan unduhan atau generasi model eksternal.

# Implementation Steps
1. **Validasi integrasi dan lokasi**
   - Periksa dependency scene, PlayerController, terrain, NavMesh data, kamera dan hierarchy; pastikan status scene tersimpan sebelum pengeditan.
   - Verifikasi collision mask, jalur playable dan shader URP tersedia; identifikasi lebih banyak kandidat dupa/peti/ritual daripada jumlah yang dibutuhkan.
   - Kebutuhan bukan rebake NavMesh hantu; gunakan mesh navigasi yang ada hanya untuk pemeriksaan keterjangkauan jika memadai, dilengkapi clearance fisik dan inspeksi manual.
   - Dependencies: persetujuan rencana.
2. **State dan interaksi terpisah**
   - Buat IInteractable, IncenseInventory, PlayerInteractor dan state dupa/peti/batu/tirta.
   - Transaksi atomic: hanya kurangi dupa sumber setelah inventori menerima; hanya kosongkan inventori setelah batu berhasil menerima.
   - Semua state dimiliki instance, tidak singleton static persisten; unsubscribe input/events pada disable/destroy.
   - Dependencies: Step 1.
3. **Prefab placeholder**
   - Buat dupa, peti, batu, pusat dan tirta dengan material URP dan collider yang sesuai. Prefab dupa dipakai baik saat lepas maupun visual yang ditaruh/dibawa, tanpa collider pickup aktif pada visual dibawa/terpasang.
   - Visual dupa dibawa boleh ditempatkan di child socket kamera terpisah dari lentera; hindari clipping dan collider pemain.
   - Dependencies: Step 2.
4. **Randomisasi aman setiap permainan**
   - Tambahkan anchors di map dan configuration jumlah exact: 3 loose, 4 dalam satu peti, 7 batu.
   - Pilih titik unik dengan jarak minimum dari spawn pemain, antar objek, peti dan pusat ritual; validasi semua titik ring dengan ground sampling, slope, obstacle clearance, dan akses.
   - Gunakan retry terbatas dan fallback layout tervalidasi agar total tetap 7, tidak hang. Log konfigurasi invalid satu kali, bukan tiap frame.
   - Sediakan seed debug untuk reproduksi; default random setiap permainan. Root runtime tidak diduplikasi setelah Play ulang, termasuk jika scene/domain reload dimatikan.
   - Dependencies: Steps 1–3.
5. **Ritual dan efek cahaya**    
   - Buat completion event saat 7 slot unik terisi; nyalakan pusat dan spawn satu tirta hanya sekali.
   - Tiga dupa lepas mendapatkan Point Light realtime hangat dengan range kecil sekitar 2–3 m dan intensity awal sekitar 0.3–0.6, shadows None; padamkan ketika diambil.
   - Pusat ritual memakai Point Light realtime putih kebiruan, range awal sekitar 6 m, fade sederhana saat completion. Nilai dituning lewat Game View tanpa merubah lampu lama.
   - Tidak menambah bloom, rebake GI atau modifikasi volume; Point Light benar-benar menerangi sekitar tanpa mengandalkan baked emission.
   - Dependencies: Steps 3–4.
6. **HUD dan koneksi scene**
   - Buat HUD minimal dengan prompt E, dibawa 0/1 dan ritual 0/7, serta feedback kondisi gagal.
   - Hubungkan kamera pemain, inventory, manager, prefab, anchors dan HUD melalui serialized fields.
   - Tirta dimiliki hanya sebagai state reward; tidak mengubah GhostRoaming/GhostFloating.
   - Dependencies: Steps 2–5.
7. **Verifikasi dan serah terima**
   - Kompilasi, EditMode tests, Play Mode manual/otomatis sesuai konfigurasi tanpa diam-diam mengaktifkan Domain Reload.
   - Inspeksi visual dari kamera pemain untuk ketampakan dupa, prompt, ring, pusat ritual dan tirta; tune hanya aset baru.
   - Hapus runner uji sementara; simpan perubahan scene dan laporkan file yang berubah serta batasan pengujian.
   - Dependencies: Steps 1–6.

# Verification & Testing
- Tepat 3 dupa lepas + 4 di satu peti pada setiap layout; tepat tujuh batu kosong awal dan tidak ada tirta awal.
- Kapasitas satu: mengambil kedua saat penuh tidak menghilangkan objek/stok; peti 4→3→2→1→0 setelah empat transfer berhasil.
- E berulang tidak menggandakan dupa, menaruh dua kali pada batu sama tidak menambah hitungan, batu kosong tanpa dupa tidak menerima.
- Invariant: jumlah loose + stok peti + dibawa + terpasang = 7 sebelum maupun sesudah ritual lengkap.
- Prompt hilang di luar jarak, diblokir dinding, dan saat cursor unlocked; target memakai kamera Main Camera yang benar.
- Pasang dupa dari sumber mana pun pada batu kosong mana pun, kemajuan akurat hingga 7/7; reward spawn dan cahaya hanya sekali.
- Tirta muncul hanya setelah completion, dapat diambil sekali, status tersimpan selama sesi; hantu tetap roaming karena combat di luar cakupan.
- Sampling banyak seed: seluruh posisi unik, clearance aman, ring radius konsisten, grounded dan terjangkau, fallback teruji saat kandidat tidak cukup. Inspeksi manual memastikan pohon terrain tidak luput dari obstacle validation.
- Restart Play dua kali dengan Domain Reload nonaktif: stok/progress reset, tidak ada duplikasi ritual/prompt/input subscriptions.
- Tidak ada Missing Script, serialized reference null, exception atau Console spam.
- Visual: lentera/movement tetap seperti semula; tiga dupa punya cahaya lembut, pusat awalnya tidak bercahaya, ketujuh batu dapat didekati dan prompt terbaca dalam landscape.
- Tes build Windows bila tersedia dan diminta; jangan mengubah build scene order, paket, define symbols, atau pengaturan grafis global tanpa kebutuhan/persetujuan tambahan.
