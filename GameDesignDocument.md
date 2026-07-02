# Game Design Document

## 1. Tong quan du an

**Ten du an:** Tower Defense 3D  
**The loai:** 3D Tower Defense, strategy, wave survival  
**Nen:** Unity, URP, Input System, NavMesh  
**Muc tieu trai nghiem:** Nhanh, ro rang, co chieu sau nang cap, tap trung vao quyet dinh dat thap, xoay camera, phan bo tai nguyen va chon thoi diem skip wave.

Du an hien tai la mot game phong thu 3D: nguoi choi dat thap tren cac o hop le, quan sat duong di cua quai, phong thu qua nhieu dot tan cong lien tiep. Game co co che tien te, nang cap thap bang ghep 2 cong trinh cung loai va cung cap, va co co che skip wave de doi lay thuong bo sung.

---

## 2. Creative Pillars

1. **Phong thu 3D co do sau khong gian**  
   Nguoi choi khong chi dat thap tren mat phang 2D, ma thao tac tren ban do 3D, xoay camera, zoom, va nhin range truc tiep.

2. **Xay dung va nang cap bang quyen quyet dinh**  
   Thap co the mua, dat, xoa co hoan tien mot phan, va ghep cap de len level cao hon. Moi quyet dinh deu lien quan den tien, tieu diet quai va vi tri.

3. **Wave pressure ro rang, nhan dien tot**  
   Game chia wave thanh cac giai doan: chuan bi, trien khai quai, chuyen sang chien dau, va ket thuc wave. Co UI thong bao wave lon, countdown, so quai con lai, va nut skip.

4. **Combat feedback day du**  
   Hieu ung mau, floating text, explosion, thanh mau cua quai, thanh mau cua thap, am thanh ban va am thanh su kien tao phan hoi nhanh.

---

## 3. Player Fantasy

Nguoi choi la chi huy phong thu mot can cu khong gian. Ban phai dung so hang co han de chong lai tung dot xam luoc, cai thien cong suat phong thu theo thoi gian, va quyet dinh luc nao nen chap nhan nguyen cuoc chien de lay thuong len wave tiep theo.

---

## 4. Core Game Loop

1. Vao scene Menu, chon Play.
2. Scene Map1 duoc load, ban co tien khoi dau va mot luong sinh menh gioi han.
3. Trong 30 giay chuan bi, nguoi choi dat thap, doi chieu, doi con tien.
4. Quai spawn tu nhieu spawn point va di theo NavMesh huong toi end point.
5. Thap tu dong tim muc tieu gan nhat trong tam ban va ban.
6. Quai bi giet trao tien, co the hien floating text va VFX no.
7. Khi wave con lai it, nut skip co the hien ra; nguoi choi co the bo qua wave de nhan thuong.
8. Neu het tat ca wave, game duoc coi la chien thang.
9. Neu quai cham end zone du sinh menh, nguoi choi thua.

---

## 5. Game Modes and Scenes

### 5.1 Scenes

- **Menu**: Man hinh chinh, choi, cai dat, thoat.
- **Map1**: Scene choi chinh hien tai.

### 5.2 Luong scene

- MenuManager load `Map1` khi bam Play.
- MenuManager va GameEndManager deu co ham quay lai `Menu`.
- GameEndManager restart scene hien tai khi bam restart.

---

## 6. Win/Lose Conditions

### 6.1 Thang

Game duoc xem la thang khi hoan thanh het tat ca waves trong WaveManager.

Luu y: trong code hien tai, phan ket thuc wave dang goi man hinh lose screen nhung hien thi text victory. Dieu nay cho thay logic end-state can dong bo lai. Ve mat y tuong, ket thuc game nen la man hinh chien thang.

### 6.2 Thua

Nguoi choi thua khi:
- `PlayerStats.currentLives` ve 0.
- Hoac end-point bi quai cham vao va bi xu ly trong EndZone.

Khi thua:
- `GameEndManager.ShowLoseScreen()` duoc goi.
- `WaveManager.GameLost()` duoc goi de khoa luong wave.
- `Time.timeScale` ve 0.

---

## 7. Player Resources and Economy

### 7.1 Tien

- Tien khoi dau trong scene: **400**.
- UI hien thi dang `Vang: X`.
- Tien tang khi giet quai.
- Tien tang khi skip wave.
- Tien giam khi mua thap.
- Khi xoa thap, nguoi choi nhan lai **50%** gia tri da dat.

### 7.2 Sourcing and spending

**Nguon tien:**
- Kill reward cua quai.
- Skip bonus khi bo qua wave.

**Chi tieu:**
- Mua tower tu shop.
- Co che ghep thap khong ton tien truc tiep, nhung chi ap dung voi thap hop le cung loai va cung level.

### 7.3 Shop behavior

- Nut shop tu dong mo khoa theo so tien hien co.
- Khi thieu tien, nut bi lam toi va khong interactable.

---

## 8. Lives / Base Health

- Bien sinh menh trong scene hien tai la **1**.
- Script PlayerStats co mac dinh `startLives = 20`, nhung scene Map1 override thanh 1.
- Neu quai vao EndZone, player mat 1 life.
- Het life thi game over.

Day la cau hinh rat khac so voi tower defense truyen thong, khien game co nhiet do cao va thoi gian sai lam rat nho.

---

## 9. Tower System

### 9.1 Dat thap

Nguoi choi dung chuot de dat thap tren tile hop le.

Quy tac dat thap:
- Khong duoc de len duong di / obstacle tilemap.
- Khong duoc de trung vi tri da co tower.
- Phai co du tien.
- Indicator se doi mau sang invalid neu vi tri khong hop le.

### 9.2 Xoa thap

- Co che chuyen sang delete mode.
- Click vao tower se mo confirmation panel.
- Xac nhan xoa se huy tower va hoan 50% cost.
- Huy xoa se dong panel.

### 9.3 Ghep / Fuse tower

Day la mot trong cac he thong dac trung nhat cua game.

Neu nguoi choi dang o build mode va click vao vi tri da co tower, game se bat sang che do keo-nhac tower:
- Tower duoc nhac len theo chuot.
- Neu tha vao mot tower khac co cung `towerType` va cung `towerLevel`, tower do duoc upgrade.
- Chi so cong don cua hai tower se duoc gop lai vao tower nhan.
- Tower con lai bi huy.
- Hien hieu ung build success.

Co che nay tao nen gia tri chien luoc dang ke: thay vi chi dat nhieu tower moi, nguoi choi co the tap trung gom suc mot vi tri manh hon.

### 9.4 Tower archetypes hien co trong scene

Trong Map1, shop dang co 4 cong trinh:

- **Turret**: cost 1000
- **Cannon**: cost 200
- **Catapult**: cost 700
- **Ballista**: cost 400

Cac prefab nay co chenh lech ve fire rate, range, va bullet prefab.

### 9.5 Tower stats and scaling

Moi tower co:
- `towerLevel`
- `damageMultiplier`
- `fireRateMultiplier`
- `rangeMultiplier`
- `baseDamage`
- `baseFireRate`

Upgrade tower:
- +1 level
- damageMultiplier + 0.5
- fireRateMultiplier + 0.2
- rangeMultiplier + 0.2
- scale model x1.2

Y nghia thiet ke:
- Tower nang cap khong chi manh hon ma con lon hon, giup nguoi choi doc duoc suc manh bang hinh anh.
- Vong range hien thi tren tower se bien doi theo rangeMultiplier.

### 9.6 Selection and validation UI

- Co indicatorRoot de hien thi o dat thap.
- Co material rieng cho trang thai hop le, invalid, delete, va fuse.
- Co success effect sau khi dat hoac fuse thanh cong.
- Co panel xac nhan khi xoa.

---

## 10. Tower Combat System

### 10.1 Targeting

Tower tu dong tim enemy gan nhat trong range.

Quy tac:
- Lien tuc query enemy co tag `Enemy`.
- Chon enemy gan nhat trong tam.
- Neu khong co muc tieu hop le, tower khong ban.

### 10.2 Firing

- Tower quay partToRotate ve phia muc tieu.
- Ban theo fireRate.
- Neu co dualFirePoints, projectile se phat ra xen ke giua cac nop ban.
- Am thanh ban duoc phat qua AudioManager theo `towerType`.

### 10.3 Bullet types

Game dang co 3 loai projectile ro rang:
- `ArrowProjectile`
- `Cannonball`
- `CatapultRock`

Tat ca deu:
- Seek target.
- Di chuyen toi muc tieu.
- Khi cham se gay damage cho EnemyHealth.

CatapultRock co duong bay cong va quay xoay, tao cam giac nang.

### 10.4 Damage scaling

Projectile nhan damage multiplier tu TowerStats:
- damage thuc te = base projectile damage x damageMultiplier
- Khi upgrade tower, cac bullet cung manh theo.

---

## 11. Enemy System

### 11.1 Enemy movement

Quai di chuyen bang NavMeshAgent.
- Spawn random tu mot trong cac spawn points.
- Duoc warp vao NavMesh gan nhat.
- Di toi end point.
- Khi game pause, agent dung lai.

### 11.2 Enemy types

Hien tai co it nhat 2 du lieu enemy trong project:

- **enemy-ufo-a Variant**: enemy thuong, HP 100, speed 1.5, khong co shooting.
- **enemy-ufo-a-weapon Variant**: enemy nang cap/boss, HP 2500, speed 0.5, co shooting.

### 11.3 Enemy attack

Enemy co the co shooting system:
- Tim tower gan nhat co tag `Tower` trong range.
- Quay phan co the ban ve tower.
- Spawn enemyBullet.
- Bullet gay damage cho TowerHealth.

### 11.4 Enemy health and rewards

EnemyHealth co:
- maxHealth
- killReward
- thanh mau fill
- hit reaction mesh / color flash
- floating text
- death explosion VFX

Khi enemy chet:
- Cong tien cho player.
- Hien floating text `+X$`.
- Chay explosion VFX.
- Huy object.

### 11.5 End-zone leak behavior

Neu enemy cham collider Finish trong EndZone:
- Player mat 1 life.
- Enemy bi huy.

---

## 12. Waves and Encounter Design

### 12.1 Wave structure

WaveManager cho phep dinh nghia:
- Wave name
- Nhieu enemy sub-group
- Moi sub-group co prefab, so luong, spawn rate

### 12.2 Wave flow

Moi wave co 4 giai doan:

1. **Preparation**  
   Countdown 30 giay, hien wave notice, mo nut next wave.

2. **Spawn burst**  
   Enemy spawn theo tung sub-group.

3. **Combat hold**  
   Cho den khi toan bo enemy chet hoac nguoi choi skip.

4. **Clear / transition**  
   Hien wave cleared, chuyen wave tiep theo.

### 12.3 Skip system

- Nut skip chi co the hien khi con it nhat `skipThreshold` quái.
- Nguoi choi bam skip se nhan `skipBonusMoney`.
- Trong scene hien tai, `skipThreshold = 5`, `skipBonusMoney = 300`.

### 12.4 Current wave content in Map1

Wave config hien tai trong scene:
- Wave 1: 100 enemy-ufo-a Variant, spawnRate 1
- Wave 2: 10 enemy-ufo-a Variant, spawnRate 2
- Wave 3: 1 enemy-ufo-a-weapon Variant, spawnRate 3

Day la cau truc co mot khoi luong lon o dau, sau do nhe hon, va ket bang boss wave.

---

## 13. Camera and Controls

### 13.1 Camera controls

- WASD hoac mui ten de pan camera.
- Mouse wheel de zoom FOV.
- Camera bi gioi han trong khoang min/max x, z va FOV.
- Khi pause hoac game over, camera dung lai.

### 13.2 Build interaction

- Click trai: dat thap, ket hop, xac nhan xoa khi delete mode.
- Click phai: huy chon tower dang build.
- Mouse hover: hien range overlay tren tower.

### 13.3 Pause

- Phim Escape mo/dong pause menu.
- Time.timeScale ve 0 khi pause.
- Resume khong mat trang thai scene.

---

## 14. UI/UX Specification

### 14.1 HUD

- Money text: `Vang: X`
- Lives text: `LIVES: X`
- Wave top text: `WAVE: current / total`
- Countdown / status text: chuan bi, con lai, hay quan dang tran ra

### 14.2 Center feedback

- Large wave notice parent de hien text lon:
  - `WAVE 1`
  - `WAVE CLEARED!`
  - `VICTORY!`
  - `DEFEAT!`

### 14.3 Confirmation dialog

- Hien khi xoa tower.
- Co confirm/cancel.

### 14.4 End screen

- GameEndPanel chua WinPanel va LosePanel.
- Restart va GoToMenu co san.

### 14.5 Tower contextual info

- Hover vao tower co the hien range visual.
- Co tower range visualization bang LineRenderer hoac cylinder visual.

---

## 15. Audio Design

AudioManager la singleton va duoc giu qua scene.

### 15.1 Music

- Background music phat tu dong khi vao game.
- Loop vo han.

### 15.2 SFX

- Moi tower type co shooting sound rieng, mapping qua ten khop voi `towerType`.
- Co build sound.
- Co error sound.
- Co win sound.
- Co lose sound.

### 15.3 Audio intent

Audio dong vai tro feedback nhanh:
- Biet luc dat thap thanh cong.
- Biet luc thieu tien.
- Biet luc tower ban.
- Biet luc ket thuc tran.

---

## 16. Visual Identity

### 16.1 Art direction

Du an dang di theo huong sci-fi / fantasy defense:
- Enemy hinh UFO.
- Tower co nhieu mau va dang:
  - cannon
  - turret
  - catapult
  - ballista
- VFX no, flash damage, floating score text.

### 16.2 Readability priorities

- Tower range phai nhin ro.
- Enemy health bar phai de thay tren dau.
- State invalid/fuse/delete phai co mau rieng.
- Wave notice phai noi bat de nguoi choi khong bo lo nhan vat cuoc chien.

---

## 17. Content Breakdown Based on Current Build

### 17.1 Maps and scene content

- 1 menu scene.
- 1 combat scene chinh.
- Map co nhieu tilemap obstacle de chi dinh vi tri dat tower.
- Co spawn points va end point duoc gan truc tiep trong scene.

### 17.2 Tower roster

- Turret
- Cannon
- Catapult
- Ballista

### 17.3 Enemy roster

- Regular UFO enemy
- Weapon UFO enemy / boss

### 17.4 VFX / support assets

- Build success effect
- Death explosion VFX
- Floating text prefab
- Health bar fill image
- Range visual for hover

---

## 18. Progression and Difficulty

Du an hien tai tien trinh theo wave co dinh, chua co meta-progression giua cac tran.

Do kho tang len chu yeu tu:
- So luong enemy trong wave.
- Enemy boss co HP rat cao.
- Sinh menh cua nguoi choi rat thap trong scene hien tai.
- Wave skip cho phep doi lay tien va giup giam ap luc, tao quyet dinh kinh te.

Neu muon nang cap game ve sau, co the them:
- Wave randomization.
- Enemy modifiers.
- Tower synergies.
- Meta upgrades.

---

## 19. Current Implementation Notes / Known Gaps

Day la nhung diem nen ghi ro trong GDD vi chung anh huong truc tiep den trang thai san pham:

- `ShowWinScreen()` hien co trong GameEndManager nhung khong thay duoc goi trong logic ket thuc wave.
- WaveManager khi het wave dang goi `ShowLoseScreen()` roi moi hien `VICTORY!`, nen end-state chua dong bo.
- `towerType` trong cac prefab dang dang la `Cannon` o data hien tai, nen am thanh shoot theo type co the chua phan loai day du neu chua sua prefab.
- Game dang co mot base scene va mot map, nen pham vi content con nho.
- Player life trong scene Map1 dang dat rat thap, chi 1.

Nhung diem tren khong phai loi thiet ke bat buoc, ma la dac ta trung thuc ve trang thai build hien tai.

---

## 20. Ideal Product Pitch

**Tower Defense 3D** la mot game phong thu 3D co nhiet do cao, trong do moi wave la mot quyet dinh ve tai nguyen. Nguoi choi dat thap, ghép thap, xoa thap, quan sat tang xung dot qua camera 3D, va canh me time skip de toi uu hoa loi ich. Game phu hop voi mot vong choi ngan, nop, va co kha nang mo rong thanh he thong tower defense day du hon voi nhieu map, enemy, boss, va meta-upgrade.

---

## 21. Suggested Next Production Steps

1. Dong bo logic end-game de thang/thua khong bi chong len nhau.
2. Chuan hoa `towerType` tren tung prefab de audio va UI hoat dong dung loai thap.
3. Bo sung data-driven wave list vao ScriptableObject neu muon scale content.
4. Nang cap menu, settings, va pause flow thanh mot hoan chinh.
5. Mo rong enemy roster va them map thu 2.

---

## 22. Summary

Ban build hien tai da co day du cac cot moi cua mot tower defense 3D:
- camera 3D
- shop tower
- dat/xoa/fuse tower
- wave system
- enemy navmesh va enemy shooting
- economy
- UI HUD
- game end
- audio manager

Noi game dang manh nhat la co che **ghep tower** va **wave skip co thuong**, day la hai he thong tao ca tinh rieng cho san pham.
