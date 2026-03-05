# 📌 Notas Personales - Refactorización DiaryApp

## 🔧 Configuración de mi Entorno

### Connection Strings que uso:

### URLs de mi API:
- **Local:** `https://localhost:7XXX/api/`
- **Azure:** `https://dev-diaryapp-c2cuanhkf2f6axee.canadacentral-01.azurewebsites.net/api/`

## 💡 Decisiones Tomadas

1. **Fecha:** 01/03/2026 - Decidí refactorizar todo para usar una API centralizada
2. **Patrón elegido:** MAUI y Web consumen la misma API REST

## ⚠️ Problemas Encontrados

- DiaryApp Web dejó de funcionar después de crear el proyecto Mobile
- Teníamos dos DbContext apuntando a la misma BD

## ✅ Lo que ya solucioné

- (Lista tus avances aquí)

---
**Repositorio:** https://github.com/pablocominiello/DiaryApp_csharp
**Rama de trabajo:** WebMobile