# 📋 Plan de Refactorización DiaryApp - Contexto Completo

**Fecha:** Marzo 2026  
**Rama Actual:** WebMobile  
**Repositorio:** https://github.com/pablocominiello/DiaryApp_csharp

---

## 🎯 Objetivo Principal

Refactorizar la arquitectura para que **DiaryApp Web** y **DiaryApp MAUI** accedan a la misma base de datos MS SQL Server a través de una API REST centralizada.

---

## 📊 Estado Actual de la Solución

### ✅ Proyectos Existentes

1. **DiaryApp (Web - Razor Pages)** - .NET 9
   - Ubicación: `/DiaryApp/`
   - Estado: ⚠️ Funciones rotas desde que se creó la versión mobile
   - Usa actualmente: `DiaryApp.Core.Data.ApplicationDbContext`
   - Tiene controladores MVC: `PersonsController`, `DiaryEntriesController`, `PaymentsController`
   - Integración con Azure Blob Storage para imágenes

2. **DiaryApp.Api (Web API REST)** - .NET 9
   - Ubicación: `/DiaryApp.Api/`
   - Estado: ✅ Funcionando
   - Usa: `DiaryApp.Api.Data.AppDbContext`
   - Tiene: `PersonsController` (funcional)
   - Configuración: Swagger, CORS habilitado, JSON con ReferenceHandler.IgnoreCycles
   - Base URL: `https://dev-diaryapp-c2cuanhkf2f6axee.canadacentral-01.azurewebsites.net/api/`

3. **DiaryApp.Mobile (MAUI)** - .NET 9
   - Ubicación: `/DiaryApp.Mobile/`
   - Estado: ✅ Consume correctamente la API para Persons
   - Plataformas: Android, iOS, MacCatalyst, Windows
   - Usa: `ApiService` que consume la API REST
   - ViewModels: `PersonsViewModel`, `PersonDetailViewModel`, etc.
   - Services: `ApiService`, `BlobStorageService`, `LocalBlobStorageService`

4. **DiaryApp.Shared** - .NET 9
   - Ubicación: `/DiaryApp.Shared/`
   - Estado: ✅ Librería de clases con modelos compartidos
   - Contiene: `Person`, `Payment`, `DiaryEntry`

5. **DiaryApp.Core** - .NET 9
   - Ubicación: `/DiaryApp.Core/`
   - Estado: ⚠️ Parcialmente implementado
   - Referenciado por: DiaryApp (Web)

---

## 🔧 Arquitectura Actual (Problemática)

### ❌ Problemas Identificados

1. **Dos DbContexts diferentes** apuntando a la misma BD
2. **Modelos duplicados**: `DiaryApp.Models` vs `DiaryApp.Shared.Models`
3. **Web dejó de funcionar** al crear la versión mobile
4. **Falta de consistencia** en el acceso a datos

---

## 🎯 Arquitectura Objetivo (Limpia)

---

## 📝 Plan de Refactorización - FASE 1: PERSONS

### Paso 1: Consolidar DbContext en DiaryApp.Core ✅

**Archivo:** `DiaryApp.Core/Data/ApplicationDbContext.cs`

- Mover/unificar el DbContext
- Usar modelos de `DiaryApp.Shared`
- Mantener seed data y configuraciones
- Eliminar `DiaryApp.Api.Data.AppDbContext` (duplicado)
- Eliminar `DiaryApp.Data.AplicationDbContext` (duplicado)

### Paso 2: Actualizar DiaryApp.Api ⏳

**Cambios necesarios:**

1. Actualizar referencia para usar `DiaryApp.Core`
2. Actualizar `Program.cs` para usar `ApplicationDbContext` de Core
3. Mantener `PersonsController` tal como está (ya funciona)
4. Verificar que endpoints respondan correctamente

**Archivo a modificar:** `DiaryApp.Api/Program.cs`

### Paso 3: Actualizar DiaryApp (Web) ⏳

**Opción A: Mantener acceso directo (Temporal)**
- Actualizar para usar `DiaryApp.Core.ApplicationDbContext`
- Mantener controladores MVC funcionando
- Permitir que siga accediendo directo al DbContext

**Opción B: Migrar a consumir API (Recomendado)**
- Crear `Services/ApiService` similar al de MAUI
- Actualizar controladores para consumir API en lugar de DbContext
- Más escalable y consistente

### Paso 4: Verificar MAUI ✅

- No requiere cambios (ya funciona correctamente)
- Continúa consumiendo la API
- Mantiene su `ApiService` actual

---

## ✅ Lo que YA Funciona

1. ✅ **MAUI consume API de Persons** correctamente
2. ✅ **API REST** expone endpoint `/api/persons` funcional
3. ✅ **Swagger** configurado en la API
4. ✅ **CORS** habilitado para MAUI
5. ✅ **Modelos compartidos** en DiaryApp.Shared
6. ✅ **Azure Blob Storage** integrado para imágenes

---

## ⚠️ Lo que Necesita Arreglarse

1. ⚠️ **DiaryApp (Web)** no puede acceder a datos
2. ⚠️ **Dos DbContexts** duplicados
3. ⚠️ **Modelos duplicados** en DiaryApp.Models
4. ⚠️ **Falta controladores API** para Payments y DiaryEntries
5. ⚠️ **Falta completar MAUI** para Payments y DiaryEntries

---

## 🚀 Próximos Pasos Inmediatos

### 1. Crear DiaryApp.Core/Data/ApplicationDbContext.cs
- Consolidar DbContext único
- Usar modelos de DiaryApp.Shared
- Mantener configuraciones y seed data

### 2. Actualizar DiaryApp.Api
- Referenciar DiaryApp.Core
- Cambiar AppDbContext por ApplicationDbContext
- Verificar PersonsController funcione

### 3. Actualizar DiaryApp (Web)
- Decidir: ¿Acceso directo o consumir API?
- Actualizar controladores
- Probar que funcione nuevamente

### 4. Crear Controladores API faltantes
- PaymentsController
- DiaryEntriesController

### 5. Completar implementación MAUI
- Terminar PaymentsViewModel/Views
- Terminar DiaryEntriesViewModel/Views

---

## 📌 Notas Importantes

- **Versión .NET:** 9.0
- **Base de Datos:** MS SQL Server (compartida)
- **Patrón funcionando:** MAUI → API REST → DbContext → SQL Server
- **Usuario:** Pablo Eugenio Cominiello
- **Persona de prueba ID 1** con fecha nacimiento: 30/06/1976

---

## 🔍 Referencias de Código Funcionando

### PersonsController (API) que funciona:

```csharp
using DiaryApp.Core.Data;
using DiaryApp.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PersonsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/persons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetPersons()
        {
            return await _context.Persons.ToListAsync();
        }

        // GET: api/persons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            var person = await _context.Persons.FindAsync(id);

            if (person == null)
            {
                return NotFound();
            }

            return person;
        }

        // POST: api/persons
        [HttpPost]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPerson", new { id = person.Id }, person);
        }

        // PUT: api/persons/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, Person person)
        {
            if (id != person.Id)
            {
                return BadRequest();
            }

            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/persons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PersonExists(int id)
        {
            return _context.Persons.Any(e => e.Id == id);
        }
    }
}
```

---

## 🎨 Diagrama de Dependencias de Proyectos

---

## 📊 Estado de Endpoints API

| Endpoint | Método | Estado | Usado por MAUI |
|----------|--------|--------|----------------|
| `/api/persons` | GET | ✅ Funciona | ✅ Sí |
| `/api/persons/{id}` | GET | ✅ Funciona | ✅ Sí |
| `/api/persons` | POST | ✅ Funciona | ✅ Sí |
| `/api/persons/{id}` | PUT | ✅ Funciona | ✅ Sí |
| `/api/persons/{id}` | DELETE | ✅ Funciona | ✅ Sí |
| `/api/payments` | GET | ⏳ Verificar | ⏳ No |
| `/api/payments/{id}` | GET | ⏳ Verificar | ⏳ No |
| `/api/diaryentries` | GET | ⏳ Verificar | ⏳ No |
| `/api/diaryentries/{id}` | GET | ⏳ Verificar | ⏳ No |

---

## 🔄 Comandos Git Útiles
