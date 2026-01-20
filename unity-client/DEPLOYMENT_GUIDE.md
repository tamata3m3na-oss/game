# دليل الإطلاق والنشر (Deployment Guide)

## المقدمة

يهدف هذا الدليل إلى توفير خطوات واضحة ومفصلة لإطلاق ونشر اللعبة في بيئة الإنتاج. يغطي الدليل جميع المراحل من التحضير للنشر إلى المراقبة بعد الإطلاق. اتباع هذه الخطوات بدقة يضمن إطلاقاً ناجحاً ويقلل من مخاطر المشاكل.

---

## القسم الأول: قائمة التحقق قبل النشر

### ١.١ التحقق من جودة الكود

قبل أي نشر، يجب التأكد من أن الكود يلتزم بمعايير الجودة المطلوبة. أولاً، يجب التأكد من عدم وجود أخطاء تجميع (compiler errors) في المشروع. ثانياً، يجب التأكد من عدم وجود تحذيرات (warnings) تشير إلى مشاكل محتملة. ثالثاً، يجب التأكد من أن جميع الاختبارات نجحت بنجاح. رابعاً، يجب التأكد من عدم وجود debug logs في الكود الإنتاجي. خامساً، يجب التأكد من أن الكود يتبع اتفاقيات التسمية والمعايير المعتمدة في المشروع.

```csharp
// ❌ مثال على كود يحتوي على debug log - يجب إزالته قبل النشر
Debug.Log($"[Debug] Player position: {position}");

// ✅ مثال على كود نظيف - مناسب للإنتاج
// لا توجدDebug logs
```

### ١.٢ التحقق من الإعدادات

تأكد من صحة جميع الإعدادات للإنتاج. يجب التحقق من أن `ServerUrl` يشير إلى خادم الإنتاج وليس التطوير. يجب التحقق من صحة رموز المصادقة وتواريخ انتهاء الصلاحية. يجب التحقق من إعدادات الشبكة مثل منافذ الاتصال ومساحات الأسماء. يجب التحقق من إعدادات الأداء مثل حجم Object Pools وعدد الخيوط.

```csharp
// في NetworkManager.cs
// ❌ إعدادات التطوير
public string ServerUrl = "ws://localhost:3000";

// ✅ إعدادات الإنتاج
public string ServerUrl = "ws://production.game.com:443";
```

### ١.٣ قائمة التحقق الكاملة

قبل النشر، راجع القائمة التالية وتأكد من выполнение جميع العناصر:

- [ ] جميع الاختبارات نجحت (unit tests, integration tests)
- [ ] لا توجد compiler errors
- [ ] لا توجد warnings غير معالجة
- [ ] الأداء مستقر (60 FPS, < 200ms latency)
- [ ] التوثيق محدث ومتوافق مع الكود
- [ ] لا توجد debug logs أو Console.WriteLine
- [ ] إعدادات الإنتاج صحيحة
- [ ] رموز API سارية المفعول
- [ ] تم اختبار الاتصال بالخادم
- [ ] تم اختبار سيناريوهات الخطأ
- [ ] تم اختبار إعادة الاتصال
- [ ] تم اختبار الأداء تحت الحمل
- [ ] تم إنشاء نسخة احتياطية من الكود
- [ ] تم توثيق التغييرات في CHANGELOG

---

## القسم الثاني: بناء للإنتاج

### ٢.١ بناء العميل (Unity Client)

#### خطوات البناء في Unity

افتح المشروع في Unity Editor واتبع الخطوات التالية لبناء نسخة للإنتاج. أولاً، انتقل إلى `File > Build Settings`. ثانياً، حدد المنصة المستهدفة (Windows, macOS, Linux, أو WebGL). ثالثاً، تأكد من إضافة جميع المشاهد المطلوبة. رابعاً، انقر على `Player Settings` وقم بتكوين الإعدادات التالية:

**إعدادات Player:**
```plaintext
Player > Resolution and Presentation:
- Default Screen Width: 1920
- Default Screen Height: 1080
- Run In Background: true

Player > Publishing Settings:
- Compression Format: Gzip (أو Disabled للتطوير)
- Data Caching: true

Player > Other Settings:
- Scripting Runtime Version: .NET Standard 2.1
- Api Compatibility Level: .NET Standard 2.1
- Allow 'unsafe' Code: false (لأسباب أمنية)
```

خامساً، انقر على `Build` وحدد مجلد الإخراج. سادساً، انتظر حتى اكتمال البناء.

#### سطر الأوامر (CLI Build)

```bash
# Windows
cd "C:\Program Files\Unity\Editor"
./Unity.exe -quit -batchmode -projectPath "C:\path\to\project" \
  -buildTarget Windows -executeMethod BuildScript.PerformBuild \
  -buildPath "C:\output\folder"

# macOS/Linux
/Applications/Unity/Hub/Editor/202X.X.X/Unity.app/Contents/MacOS/Unity \
  -quit -batchmode -projectPath "/path/to/project" \
  -buildTarget Mac -executeMethod BuildScript.PerformBuild \
  -buildPath "/output/folder"
```

### ٢.٢ بناء الخادم (Backend)

#### Node.js

```bash
# الانتقال لمجلد الخادم
cd backend

# تثبيت зависимостей
npm install

# بناء TypeScript
npm run build

# أو التطوير مع إعادة التحميل
npm run dev
```

#### التحقق من البناء

```bash
# تشغيل اختبارات الوحدة
npm test

# التحقق من نوع TypeScript
npm run type-check

# تحليل الكود
npm run lint
```

### ٢.٣ حزمة النشر

بعد البناء، يجب إنشاء حزمة نشر منظمة تحتوي على جميع الملفات اللازمة:

```
deployment-package/
├── client/
│   ├── GameClient.exe
│   ├── GameClient_Data/
│   │   ├── Managed/
│   │   ├── Resources/
│   │   ├── level0
│   │   └── ...
│   └── README.txt
├── server/
│   ├── dist/
│   │   ├── index.js
│   │   └── ...
│   ├── package.json
│   └── ecosystem.config.js
├── config/
│   ├── server-config.json
│   └── client-config.json
└── scripts/
    ├── start-server.sh
    ├── start-client.sh
    └── health-check.sh
```

---

## القسم الثالث: نشر الخادم

### ٣.١ إعداد البيئة

#### متطلبات النظام

| المكون | الحد الأدنى | الموصى به |
|--------|-------------|-----------|
| CPU | 2 cores | 4+ cores |
| RAM | 2 GB | 4+ GB |
| Storage | 10 GB SSD | 50+ GB SSD |
| Network | 10 Mbps | 100+ Mbps |

#### تثبيت相依يات

```bash
# تحديث النظام
sudo apt update && sudo apt upgrade -y

# تثبيت Node.js
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# تثبيت PM2 لإدارة العمليات
sudo npm install -g pm2

# تثبيت Nginx (كـ reverse proxy)
sudo apt install nginx -y
```

### ٣.٢ نشر الخادم

#### الطريقة الأولى: باستخدام PM2

```bash
# نسخ الملفات
scp -r build/* user@server:/opt/game-server/

# الدخول للخادم
ssh user@server

# تثبيت相依يات
cd /opt/game-server
npm install --production

# تشغيل باستخدام PM2
pm2 start ecosystem.config.js
pm2 startup
pm2 save

# التحقق من الحالة
pm2 status
pm2 logs
```

#### ملف ecosystem.config.js

```javascript
module.exports = {
  apps: [{
    name: 'game-server',
    script: 'dist/index.js',
    instances: 'max',
    exec_mode: 'cluster',
    watch: false,
    max_memory_restart: '500M',
    env: {
      NODE_ENV: 'production',
      PORT: 3000
    },
    error_file: '/var/log/game-server/error.log',
    out_file: '/var/log/game-server/out.log',
    log_file: '/var/log/game-server/combined.log'
  }]
};
```

#### الطريقة الثانية: Docker

```dockerfile
# Dockerfile
FROM node:18-alpine

WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production

COPY dist ./dist

EXPOSE 3000

CMD ["node", "dist/index.js"]
```

```bash
# بناء ونشر
docker build -t game-server:latest .
docker run -d -p 3000:3000 --name game-server game-server:latest
```

### ٣.٣ إعداد Nginx كـ Reverse Proxy

```nginx
# /etc/nginx/sites-available/game-server
server {
    listen 80;
    server_name api.game.com;

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        
        # WebSocket proxying
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
    
    # Health check endpoint
    location /health {
        proxy_pass http://localhost:3000/health;
    }
}
```

```bash
# تفعيل الإعدادات
sudo ln -s /etc/nginx/sites-available/game-server /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### ٣.٤ إعداد SSL/HTTPS

```bash
# تثبيت Certbot
sudo apt install certbot python3-certbot-nginx -y

# الحصول على شهادة SSL
sudo certbot --nginx -d api.game.com

# التحقق من التجديد التلقائي
sudo certbot renew --dry-run
```

---

## القسم الرابع: نشر العميل

### ٤.١ التحديثات التلقائية

#### نظام التحديث

```csharp
public class UpdateManager : MonoBehaviour
{
    private const string UPDATE_URL = "https://api.game.com/version";
    private const string DOWNLOAD_URL = "https://api.game.com/download";
    
    public void CheckForUpdates()
    {
        StartCoroutine(CheckVersion());
    }
    
    private IEnumerator CheckVersion()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(UPDATE_URL))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var versionData = JsonUtility.FromJson<VersionData>(request.downloadHandler.text);
                
                if (versionData.version > Application.version)
                {
                    ShowUpdateDialog(versionData);
                }
            }
        }
    }
}
```

### ٤.٢ التوزيع

#### منصات التوزيع

**Steam:**
1. إعداد Steamworks SDK
2. بناء نسخة Steam
3. رفع الملفات عبر Steamworks Partner
4. إعداد Depot configuration
5. نشر الباتا

** itch.io:**
1. ضغط الملفات بـ zip
2. رفع على itch.io
3. تعيين قناة التحديث

**WebGL:**
1. رفع ملفات البناء للخادم
2. تكوين Service Worker للتخزين المؤقت
3. إعداد CDN للتوزيع

---

## القسم الخامس: المراقبة بعد الإطلاق

### ٥.١ مراقبة الأداء

#### مقاييس النظام

```bash
# مراقبة استخدام الموارد
htop

# مراقبة الشبكة
iftop

# مراقبة القرص
iotop

# مراقبة الاتصالات
netstat -tulpn
```

#### logs

```bash
# عرض logs الخادم
tail -f /var/log/game-server/combined.log

# عرض أخطاء
tail -f /var/log/game-server/error.log

# تحليل الأخطاء
grep "ERROR" /var/log/game-server/error.log | tail -20
```

#### لوحة المراقبة (Dashboard)

أنشئ لوحة مراقبة باستخدام Grafana:

```yaml
# prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'game-server'
    static_configs:
      - targets: ['localhost:9090']
```

### ٥.٢ مقاييس الأداء الرئيسية

#### مقاييس الشبكة

| المقياس | الهدف | تحذير | حرج |
|---------|-------|-------|-----|
| Latency (avg) | < 100ms | > 150ms | > 200ms |
| Latency (max) | < 200ms | > 300ms | > 500ms |
| Packet loss | < 0.1% | > 0.5% | > 1% |
| Connection errors | 0 | > 5/min | > 20/min |

#### مقاييس الخادم

| المقياس | الهدف | تحذير | حرج |
|---------|-------|-------|-----|
| CPU Usage | < 50% | > 70% | > 90% |
| Memory Usage | < 60% | > 80% | > 95% |
| Response time | < 50ms | > 100ms | > 200ms |
| Error rate | < 0.1% | > 1% | > 5% |

### ٥.٣ التنبيهات

```yaml
# alertmanager.yml
route:
  group_by: ['alertname']
  receiver: 'default'

receivers:
  - name: 'default'
    email_configs:
      - to: 'alerts@game.com'
        send_resolved: true

inhibit_rules:
  - source_match:
      severity: 'critical'
    target_match:
      severity: 'warning'
    equal: ['alertname']
```

---

## القسم السادس: Rollback والإصلاح

### ٦.١ إجراءات Rollback

#### Rollback الخادم

```bash
# عرض الإصدارات السابقة
pm2 list

# Rollback للإصدار السابق
pm2 resurrect

# أو تحديد إصدار محدد
pm2 delete game-server
pm2 start game-server@previous
```

#### Rollback العميل

```bash
# نسخ الإصدار القديم
cp -r /backup/client-v1.0.0 /var/www/game-client/

# تحديث الروابط
ln -sf /var/www/game-client-v1.0.0 /var/www/game-client/current
```

### ٦.٢ خطة الطوارئ

#### سيناريوهات الطوارئ

| السيناريو | الإجراء |
|-----------|---------|
| الخادم لا يستجيب | التحقق من logs، إعادة تشغيل بـ PM2 |
| استهلاك ذاكرة عالي | تحليل التسريبات، زيادة RAM أو تحسين الكود |
| هجمات DDoS | تفعيل CDN protection، تصفية IP |
| قاعدة بيانات معطلة | تفعيل replica، Rollback للتغييرات |
| فقدان البيانات | استعادة من backup |

#### قائمة التحقق للطوارئ

- [ ] فريق الدعم جاهز
- [ ] قنوات الاتصال معمّرة (Slack, Email)
- [ ] إجراءات Rollback موثقة
- [ ] نسخ احتياطية حديثة
- [ ] خطة تواصل مع اللاعبين

---

## القسم السابع: الوثائق الدورية

### ٧.١ تقرير الإطلاق

بعد كل إطلاق، أنشئ تقريراً يحتوي على:

```markdown
# تقرير الإطلاق - الإصدار X.X.X

## المعلومات العامة
- **التاريخ:** YYYY-MM-DD
- **الوقت:** HH:MM UTC
- **الإصدار:** X.X.X
- **المسؤول:** الاسم

## التغييرات
- قائمة التغييرات الرئيسية
- التحسينات المضافة
- الأخطاء المصححة

## حالة النشر
- [ ] الخادم نعم/لا
- [ ] العميل نعم/لا
- [ ] جميع الخدمات تعمل

## المقاييس
- عدد اللاعبين المتصلين
- متوسط التأخير
- معدل الأخطاء

## المشاكل المعروفة
- قائمة المشاكل المفتوحة
- الحلول المؤقتة

## الإجراءات المطلوبة
- مهام المتابعة
```

### ٧.٢ صيانة دورية

####每日
- مراجعة logs للأخطاء
- التحقق من مقاييس الأداء

#### أسبوعياً
- تحليل trends الأداء
- مراجعة أخطاء المستخدمين
- تحديث الشهادات إذا لزم الأمر

#### شهرياً
- مراجعة وتحديث الوثائق
- اختبار إجراءات Rollback
- مراجعة إعدادات الأمان
- تحديث依赖إذا لزم الأمر

---

## الملحق: الأوامر السريعة

```bash
# === إدارة الخادم ===
ssh user@server

# حالة PM2
pm2 status
pm2 logs --lines 50

# إعادة تشغيل
pm2 restart game-server

# إيقاف
pm2 stop game-server

# === مراقبة ===
htop
netstat -tulpn | grep 3000

# === Nginx ===
sudo nginx -t
sudo systemctl reload nginx

# === SSL ===
sudo certbot renew --dry-run

# === النسخ الاحتياطي ===
pg_dump game_db > backup.sql
tar -czvf backup-$(date +%Y%m%d).tar.gz /opt/game-server
```

---

## الخاتمة

يوفر هذا الدليل إطاراً شاملاً لإطلاق ونشر اللعبة في بيئة الإنتاج. اتباع هذه الخطوات يضمن إطلاقاً ناجحاً ويقلل من المخاطر. تذكر دائماً أن تبدأ باختبار شامل، وتراقب المقاييس بعد الإطلاق، وتكون مستعداً لل Rollback إذا لزم الأمر.

إذا واجهت أي مشكلة غير مغطاة في هذا الدليل، راجع فريق الدعم أو راجع الوثائق التقنية الأخرى.

---

*تم إنشاء هذا الدليل لضمان عملية نشر آمنة وموثوقة للعبة.*
