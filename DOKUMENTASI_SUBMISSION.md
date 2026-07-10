# Dokumentasi Submission — Novastra Test

## 1. Ringkasan Project

Novastra Test adalah prototype battle turn-based di Unity yang menggabungkan sistem pertarungan berbasis kecepatan, pemilihan target, skill berbasis data, AI musuh sederhana, Live2D, visual feedback melalui post-processing, dan integrasi visual novel menggunakan Naninovel.

Tujuan utama prototype ini adalah membangun fondasi battle yang modular dan mudah dikembangkan. Unit, statistik, skill, urutan aksi, dan konfigurasi encounter dipisahkan dari logika runtime sehingga konten baru dapat ditambahkan melalui Unity Inspector tanpa menulis ulang alur utama battle.

### Fitur yang tersedia

- Battle otomatis disiapkan ketika Play Mode dimulai.
- Tim Player dan Enemy dibentuk berdasarkan konfigurasi battle.
- Turn ditentukan oleh nilai SPD masing-masing unit.
- UI menampilkan lima urutan turn: turn terdekat di bawah dan turn yang lebih jauh di atas.
- Karakter dapat muncul berulang pada preview apabila kecepatannya menghasilkan lebih dari satu giliran dalam lima turn berikutnya.
- Player dapat memilih skill dan target menggunakan Input System.
- Enemy memilih skill dan target yang valid secara acak.
- Skill mendukung single target, multiple target, all target, dan self target.
- Action skill disusun sebagai sequence, misalnya animasi, menunggu, damage, atau heal.
- Sistem event menangani perubahan state, damage, heal, death, dan pergantian turn.
- Naninovel dapat mengambil alih presentasi sebelum battle, lalu mengembalikan kontrol ke battle.
- Post-processing memberi feedback ketika menerima damage, heal, kondisi bahaya, victory, dan defeat.

## 2. Gambaran Desain Sistem

### Arsitektur utama

Sistem dibagi berdasarkan tanggung jawab agar setiap bagian dapat dikembangkan secara terpisah:

| Sistem | Tanggung jawab |
| --- | --- |
| `BattleManager` | Menyiapkan battle, menyimpan kedua tim, mengatur state, validasi target, serta menentukan victory/defeat. |
| `TurnManager` | Mengelola timeline berbasis SPD, menentukan unit aktif, dan membuat preview lima turn tanpa mengubah timeline asli. |
| `BattleInputManager` | Mengatur input player, alur eksekusi skill, delay enemy, serta keputusan acak skill dan target enemy. |
| `TargetingManager` | Menyimpan kandidat target, mengatur target aktif, outline, perpindahan target, dan pemilihan target acak. |
| `Unit` dan `UnitHealth` | Menyimpan data runtime unit, statistik, faction, health, dan status hidup. |
| `UnitConfig`, `SkillConfig`, `BattleConfig` | Menyimpan data konten menggunakan ScriptableObject. |
| `SkillAction` | Menyusun efek skill sebagai rangkaian action yang modular. |
| `TurnOrderUI` | Memperbarui lima blok UI tetap berdasarkan simulasi timeline. |
| `BattleVisualNovelCoordinator` | Menjalankan scene Naninovel secara additive dan melanjutkan state battle setelah narasi selesai. |
| `BattlePostProcessController` | Memberikan feedback visual berdasarkan event battle. |

### Alur battle

1. Ketika Play Mode dimulai, `BattleManager.Start()` menjalankan setup battle satu kali.
2. Unit Player dan Enemy di-spawn, diinisialisasi dari `UnitConfig`, lalu didaftarkan ke tim masing-masing.
3. State berpindah ke `Setup` dan `TurnManager` membangun timeline seluruh unit hidup.
4. Jika terdapat script Naninovel pembuka, battle masuk ke `VisualNovelPause`. Jika tidak, battle langsung masuk ke `TurnStart`.
5. `TurnManager` memilih unit dengan action value terendah dan mengirim event `OnTurnStart`.
6. Jika unit aktif adalah Player, sistem menunggu pemilihan skill dan target. Jika unit aktif adalah Enemy, AI memilih skill serta target secara acak.
7. Skill dijalankan sebagai sequence action, kemudian sistem memeriksa kondisi victory atau defeat.
8. Apabila battle belum selesai, action value unit aktif di-reset dan timeline dilanjutkan ke turn berikutnya.

### Desain state machine

Battle menggunakan state berikut:

- `Setup`: mendaftarkan unit ke timeline.
- `VisualNovelPause`: menghentikan alur battle sementara Naninovel berjalan.
- `TurnStart`: menentukan unit berikutnya.
- `WaitingForInput`: menunggu input Player atau delay keputusan Enemy.
- `ResolvingActions`: menjalankan skill terpilih.
- `CheckingBattleEnd`: mengakhiri turn dan memeriksa kelanjutan battle.
- `Victory` dan `Defeat`: mengakhiri battle dan menjalankan feedback visual akhir.

Perubahan state dikirim melalui event. Pendekatan ini mengurangi ketergantungan langsung antarkomponen dan memungkinkan UI, input, visual novel, maupun post-processing merespons perubahan yang sama secara mandiri.

## 3. Desain Turn dan SPD

Desain SPD mencoba mengikuti rasa timeline Honkai: Star Rail sedekat mungkin dalam ruang lingkup prototype. Pendekatan ini dibuat melalui riset mendalam dengan bantuan GPT untuk memahami hubungan antara SPD, action value, dan frekuensi turn. Implementasi ini merupakan interpretasi untuk kebutuhan prototype, bukan reproduksi formula internal resmi game tersebut.

Nilai dasar setiap aksi dihitung dengan rumus:

```text
Base Action Value = 10000 / max(1, SPD)
```

Unit dengan SPD lebih tinggi memperoleh base action value lebih kecil, sehingga mencapai turn lebih cepat dan berpotensi muncul lebih sering pada timeline.

Saat memilih turn berikutnya:

1. Sistem mencari unit hidup dengan current action value terendah.
2. Nilai terendah tersebut dianggap sebagai waktu yang telah berlalu.
3. Waktu tersebut dikurangi dari seluruh entry timeline.
4. Unit terpilih menjalankan turn.
5. Setelah selesai, action value unit tersebut dikembalikan ke base action value miliknya.

Preview UI mensimulasikan lima pemilihan berikutnya menggunakan salinan action value. Karena hanya data salinan yang diubah, membuka atau memperbarui UI tidak pernah memajukan timeline battle yang sebenarnya.

## 4. Data dan Skill

### ScriptableObject

Konten utama disimpan sebagai ScriptableObject:

- `UnitConfig` menyimpan nama, prefab, visual, statistik, dan daftar skill.
- `SkillConfig` menyimpan tipe targeting, faction target, jumlah target, dan sequence action.
- `BattleConfig` menyimpan komposisi enemy dan script Naninovel pembuka.

Pemisahan data dan logic dipilih agar balancing SPD, HP, damage, komposisi tim, serta urutan action skill dapat dilakukan melalui Inspector tanpa mengubah kode.

### Skill action sequence

Setiap skill memiliki daftar polymorphic `SkillAction`. Action yang tersedia mencakup:

- animation override;
- wait/delay;
- damage;
- heal.

Semua action menerima `SkillExecutionContext` yang sama, berisi caster, target, dan konfigurasi skill. Struktur ini memudahkan penambahan action baru seperti buff, debuff, status effect, atau resource modification.

### Enemy AI

AI sengaja dibuat ringan untuk prototype:

- memilih secara acak dari seluruh skill non-null milik unit;
- membangun kandidat target berdasarkan aturan skill;
- memilih target valid secara acak untuk single/multiple targeting;
- tidak mengubah perilaku skill `Self` dan `AllTargets`;
- melewati turn dengan aman apabila unit tidak memiliki skill atau target valid.

Pendekatan acak dipilih untuk memberikan variasi perilaku tanpa menambahkan decision tree kompleks sebelum fondasi battle selesai.

## 5. Keputusan Teknis

### Unity dan komponen modular

Unity dipilih karena mendukung workflow berbasis component, ScriptableObject, coroutine, Input System, URP, serta integrasi asset pihak ketiga yang dibutuhkan prototype. Sistem dipisahkan menjadi manager, data, event, unit, skill action, dan presentation agar tanggung jawab setiap class tetap jelas.

### Naninovel

Naninovel dipilih karena proses setup relatif mudah dan alurnya straightforward. Integrasi dilakukan menggunakan scene additive agar sistem visual novel dapat berjalan ketika diperlukan tanpa mencampur seluruh object Naninovel ke scene battle utama. Setelah script selesai, scene visual novel dilepas dan battle kembali ke state yang telah ditentukan.

### Gamepangin

Gamepangin digunakan untuk pola umum seperti singleton, event messaging, data definition, dan object hook. Selain membantu general design dan mengurangi boilerplate, Gamepangin merupakan tools yang sudah dipercaya dan digunakan sejak masa awal pengalaman kerja, sehingga workflow serta pola penggunaannya sudah familiar.

### Event-driven communication

Event digunakan untuk komunikasi lintas sistem, misalnya `OnTurnStart`, `OnChangeBattleState`, `OnTakeDamage`, `OnHeal`, dan `OnDeath`. Keputusan ini membuat producer tidak perlu mengetahui seluruh consumer. Sebagai contoh, damage dapat sekaligus memperbarui health, post-processing, dan kondisi battle tanpa memasukkan semua dependensi tersebut ke skill action.

### Lean Pool

Unit di-spawn melalui Lean Pool agar lifecycle GameObject dapat dikembangkan menuju reuse object dan mengurangi biaya instantiate/destroy apabila battle menjadi lebih besar atau sering dimuat ulang.

### Fixed-slot turn UI

Turn order menggunakan lima block dan text yang sudah tersedia di scene. Ketika timeline berubah, sistem hanya mengganti text, warna, dan status aktif. Tidak ada spawn UI setiap pergantian turn, sehingga implementasi lebih sederhana, stabil, dan cukup efisien untuk jumlah slot yang tetap.

## 6. Feedback Visual dan Presentasi

- Model karakter ditampilkan menggunakan workflow visual yang mendukung Live2D.
- Outline menunjukkan target yang sedang dipilih.
- Volume post-processing digunakan sebagai pulse ketika Player menerima damage atau unit menerima heal.
- Danger effect aktif ketika hanya tersisa satu Player dengan health di bawah 30%.
- Victory dan defeat memiliki transisi volume terpisah.
- UI turn order menggunakan warna biru untuk Player dan merah untuk Enemy serta menampilkan nama dan faction.

## 7. Cara Menjalankan

1. Buka project menggunakan Unity `6000.3.14f1`.
2. Buka scene `Assets/_Production/Scenes/Main Scene.unity`.
3. Pastikan `BattleManager` memiliki `BattleConfig`, daftar Player, dan positioning hooks yang valid.
4. Tekan Play.
5. Battle akan setup secara otomatis dan masuk ke visual novel pembuka atau turn pertama.

Kontrol fallback keyboard:

- Skill 1: `L` atau `1`.
- Skill 2: `K` atau `2`.
- Target sebelumnya: `Left Arrow` atau `A`.
- Target berikutnya: `Right Arrow` atau `D`.
- Konfirmasi: `Enter` atau `Space`.

## 8. Batasan dan Pengembangan Lanjutan

Prototype saat ini berfokus pada fondasi sistem. Beberapa pengembangan lanjutan yang memungkinkan:

- AI berbobot berdasarkan kondisi health, faction, dan fungsi skill;
- buff, debuff, status effect, energy, dan cooldown;
- formula damage yang lebih lengkap;
- character selection dan save data untuk menggantikan daftar Player sementara;
- animasi perpindahan slot turn order;
- hasil battle, reward, dan transisi menuju encounter berikutnya;
- automated tests khusus untuk simulasi timeline dan targeting.

## 9. Saran Isi Video Demo (1–3 Menit)

1. Tampilkan konfigurasi `UnitConfig`, `SkillConfig`, dan `BattleConfig` secara singkat.
2. Tekan Play dan tunjukkan setup battle otomatis.
3. Tampilkan perpindahan dari Naninovel menuju battle.
4. Jelaskan UI lima turn dan pengaruh SPD terhadap urutan.
5. Demonstrasikan pemilihan skill serta target Player.
6. Tunjukkan Enemy memilih skill dan Player target secara acak.
7. Tampilkan feedback damage, heal, danger, dan victory/defeat bila waktunya memungkinkan.

---

Dokumen ini disertakan bersama Unity Project/source code sebagai penjelasan singkat mengenai desain sistem dan keputusan teknis prototype Novastra Test.
