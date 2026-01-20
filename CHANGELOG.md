# سجل التغييرات / Changelog

جميع التغييرات المهمة لهذا المشروع سيتم توثيقها في هذا الملف.
All notable changes to this project will be documented in this file.

---

## [2025-01-20] - إصلاح إصدار Unity وخطأ MonoManager NULL
## [2025-01-20] - Unity Version Fix and MonoManager NULL Error Resolution

### ✨ تم إضافته / Added
- سكربت تنظيف شامل للمشروع: `unity-client/clean_project.sh` (Linux/Mac)
- سكربت تنظيف للمشروع: `unity-client/clean_project.bat` (Windows)
- سكربت التحقق من المشروع: `unity-client/verify_project.sh`
- سجل تفصيلي للإصلاح: `UNITY_FIX_LOG.md`
- دليل ثنائي اللغة: `unity-client/UNITY_VERSION_FIX_README.md`
- تعليمات سريعة بالعربية: `unity-client/QUICK_FIX_INSTRUCTIONS_AR.md`
- ملخص التحقق: `PROJECT_VERIFICATION_SUMMARY.md`

### 🔄 تم تغييره / Changed
- تحديث إصدار المشروع من 2022.3.10f1 إلى 2022.3.62f3
  - File: `unity-client/ProjectSettings/ProjectVersion.txt`

### ✅ تم إصلاحه / Fixed
- حل مشكلة خطأ "GetManagerFromContext: pointer to object of manager 'MonoManager' is NULL"
- تحسين توافق الحزم مع Unity 2022.3.62f3
- تنظيف مجلدات Unity المتولدة (Library, Temp, obj, Logs, UserSettings)

### 🔍 تم التحقق منه / Verified
- جميع الحزم متوافقة مع Unity 2022.3.62f3
- manifest.json صحيح ويتضمن جميع التبعيات المطلوبة
- هيكل المشروع سليم (Assets/Scripts, Assets/Scenes, Assets/Prefabs)
- إعدادات المشروع (Physics, Quality, Graphics) صحيحة

### 📦 الحزم النهائية / Final Packages
- com.unity.inputsystem: 1.7.0
- com.unity.textmeshpro: 3.0.6
- com.unity.ugui: 1.0.0
- com.unity.addressables: 1.19.19
- com.unity.render-pipelines.universal: 14.0.7
- com.unity.nuget.newtonsoft-json: 3.2.1
- com.demigiant.dotween: 1.2.705

---

## التنسيق / Format

التنسيق المستند إلى [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

### الأنواع / Types
- ✨ تم إضافته / Added - ميزات جديدة
- 🔄 تم تغييره / Changed - تغييرات في الوظائف الموجودة
- ❌ تم إزالته / Deprecated - ميزات ستتم إزالتها قريباً
- ❌ تم حذفه / Removed - ميزات تمت إزالتها
- ✅ تم إصلاحه / Fixed - إصلاح الأخطاء
- 🔒 الأمان / Security - إصلاحات أمنية
- 🔍 تم التحقق منه / Verified - تم التحقق من صحة المكونات
