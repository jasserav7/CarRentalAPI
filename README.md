# CarRental API

API REST para la gestión de alquiler de autos, construida con **C# y .NET 10 (ASP.NET Core Web API)**, **Entity Framework Core** y **SQL Server**.

Proyecto de aprendizaje: permite administrar autos y usuarios, y registrar alquileres con validación de fechas, control de disponibilidad y cálculo automático del precio total.

## Tecnologías

- C# / .NET 10, ASP.NET Core Web API (controladores)
- Entity Framework Core 10 (Code First, migraciones, LINQ)
- SQL Server (LocalDB / SQL Express)
- Patrón DTO, inyección de dependencias, `async/await`
- OpenAPI para documentación de la API

## Estructura del proyecto

```
CarRentalAPI/
├── CarRentalAPI.slnx
├── CarRental.Tests/    # Pruebas unitarias (Services/) y de integración (Integration/)
└── CarRental.Api/
    ├── Controllers/    # Endpoints: Cars, Users, Rentals, Reports
    ├── Services/       # Lógica de negocio y acceso a datos (Car, User, Rental, Report)
    ├── Models/         # Entidades de base de datos (Car, User, Rental)
    ├── Dto/            # Objetos de transferencia expuestos por la API
    ├── Data/           # AppDbContext (EF Core)
    ├── Migrations/     # Migraciones de la base de datos
    ├── SeedData.cs     # Datos de prueba iniciales
    ├── CarRental.Api.http  # Peticiones de ejemplo
    └── Program.cs
```

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- SQL Server (por ejemplo SQL Server Express)

## Cómo ejecutarlo

1. Clona el repositorio y entra a la carpeta del proyecto:

   ```bash
   cd CarRentalAPI/CarRental.Api
   ```
2. Crea tu configuración local copiando la plantilla y ajusta la cadena de conexión a tu instancia de SQL Server:

   ```bash
   cp appsettings.example.json appsettings.Development.json
   ```

   ```json
   "ConnectionStrings": {
     "Database": "Server=YOUR_SERVER;Database=CarRentalDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

   `appsettings.Development.json` está en el `.gitignore`, así que tus datos de conexión nunca se suben al repositorio. Alternativa: `dotnet user-secrets set "ConnectionStrings:Database" "<cadena>"`.
3. Crea la base de datos aplicando las migraciones (requiere la herramienta `dotnet-ef`: `dotnet tool install --global dotnet-ef`):

   ```bash
   dotnet ef database update
   ```
4. Ejecuta la API:

   ```bash
   dotnet run
   ```

   Queda disponible en `http://localhost:5259` (o `https://localhost:7285`). Al arrancar se cargan usuarios y autos de ejemplo si las tablas están vacías.
5. En desarrollo, el documento OpenAPI está en `/openapi/v1.json`. En [CarRental.Api.http](CarRental.Api/CarRental.Api.http) hay peticiones listas para probar desde VS Code (REST Client) o Visual Studio.

## Endpoints

### Cars

| Método | Ruta               | Descripción          |
| ------- | ------------------ | --------------------- |
| GET     | `/api/cars`      | Lista todos los autos |
| GET     | `/api/cars/{id}` | Obtiene un auto       |
| POST    | `/api/cars`      | Crea un auto          |
| PUT     | `/api/cars/{id}` | Actualiza un auto     |
| DELETE  | `/api/cars/{id}` | Elimina un auto       |

### Users

| Método | Ruta                | Descripción       |
| ------- | ------------------- | ------------------ |
| GET     | `/api/users`      | Lista los usuarios |
| GET     | `/api/users/{id}` | Obtiene un usuario |
| POST    | `/api/users`      | Crea un usuario    |
| PUT     | `/api/users/{id}` | Actualiza un usuario (el `id` del cuerpo debe coincidir con el de la ruta) |
| DELETE  | `/api/users/{id}` | Elimina un usuario; `409` si tiene alquileres |
| GET     | `/api/users/{id}/rentals` | Historial de alquileres del usuario |

### Rentals

| Método | Ruta             | Descripción     |
| ------- | ---------------- | ---------------- |
| GET     | `/api/rentals` | Lista alquileres. Filtros opcionales: `userId`, `carId`, `from`, `to` |
| GET     | `/api/rentals/{id}` | Detalle de un alquiler, con usuario y auto |
| POST    | `/api/rentals` | Crea un alquiler |

`from` y `to` devuelven los alquileres que se solapan con esa ventana de fechas (`400` si `from` no es anterior a `to`).

Ejemplo de alquiler:

```json
POST /api/rentals
{
  "userId": 1,
  "carId": 2,
  "startDate": "2027-01-10T10:00:00",
  "endDate": "2027-01-14T10:00:00"
}
```

Respuesta: `{ "id": 1, "totalPrice": 200.00 }` (días × precio diario del auto).

### Reports

| Método | Ruta                          | Descripción |
| ------- | ----------------------------- | ----------- |
| GET     | `/api/reports/summary`        | Total de alquileres, ingresos totales y auto más alquilado |
| GET     | `/api/reports/revenue-by-car` | Alquileres, días e ingresos por auto, de mayor a menor ingreso |

## Reglas de negocio

- **Antigüedad máxima:** no se puede registrar ni actualizar un auto con más de 5 años de antigüedad (`400 Bad Request`).
- **Período válido:** la fecha de inicio debe ser anterior a la de fin y el alquiler debe durar al menos un día (`400`).
- **Usuario y auto existentes:** al crear un alquiler se valida que ambos existan (`404 Not Found`).
- **Disponibilidad:** un auto no puede alquilarse en un período que se solape con otro alquiler existente (`409 Conflict`).
- **Precio total:** se calcula como número de días × precio diario y se guarda con el alquiler.
- **Usuarios con alquileres:** un usuario con alquileres no se puede eliminar (`409`), para no perder el historial.

## Pruebas

Desde la raíz del repositorio:

```bash
dotnet test
```

Las pruebas no necesitan SQL Server: usan SQLite en memoria con el esquema real. Hay pruebas unitarias de los servicios (`CarRental.Tests/Services`) y pruebas de integración que levantan la API completa y hacen peticiones HTTP (`CarRental.Tests/Integration`).

## Mejoras pendientes

- [x] Actualizar y eliminar usuarios (CRUD completo)
- [x] Endpoints para consultar alquileres y generar informes
- [ ] Autenticación y autorización (JWT)
- [x] Pruebas unitarias y de integración
- [ ] Contenerización con Docker
