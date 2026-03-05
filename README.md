## 📁 Estructura del Proyecto

### DiaryApp.Shared
Modelos compartidos entre todos los proyectos (Web, API, MAUI)
- `Models/Person.cs`
- `Models/Payment.cs`
- `Models/DiaryEntry.cs`

### DiaryApp.Core
Capa de acceso a datos compartida
- `Data/ApplicationDbContext.cs` - DbContext de Entity Framework
- `Interfaces/IBlobStorageService.cs` - Interfaz para Azure Blob Storage

### DiaryApp.Api
API REST para acceso desde aplicaciones móviles
- Base URL: `https://dev-diaryapp-c2cuanhkf2f6axee.canadacentral-01.azurewebsites.net/api/`
- Endpoints: `/persons`, `/payments`, `/diaryentries`

### DiaryApp (Web)
Aplicación web Razor Pages para gestión administrativa
- Paginación con Bootstrap
- Integración con Azure Blob Storage

### DiaryApp.Mobile
Aplicación MAUI multiplataforma (Android/iOS/Mac)
- Consume DiaryApp.Api vía REST
- Soporte offline con caché local
