# Short-Term Stay API Project

A comprehensive .NET 8 Web API for a short-term stay platform (Airbnb-like), featuring Host, Guest, and Admin functionalities with JWT authentication. The solution consists of two projects: the **main API** (controllers, services, EF Core) and an **Ocelot API Gateway** for routing, rate limiting, and Swagger aggregation.

## 🔗 Project Links
- **GitHub Repository**: [https://github.com/turkbeyza/short-term-stay]
- **Deployed Swagger UI**: [https://wa-short-term-gateway-faegbyf9exh7d7bz.switzerlandnorth-01.azurewebsites.net/swagger/index.html]
- **Load Test Report**: [LOAD-TEST-REPORT.md](load-test/LOAD-TEST-REPORT.md)
- **Presentation Video**: [google.com]
---

## 🏗️ Design & Architecture

The project is built using a **Service-Oriented Architecture** to ensure separation of concerns and maintainability.

- **Backend**: .NET 8 Web API.
- **Database**: SQL Server with Entity Framework Core.
- **Authentication**: JWT (JSON Web Token) with Role-Based Access Control (Host, Guest, Admin).
- **API Gateway**: Ocelot Gateway for routing and future rate limiting.
- **Documentation**: Swagger/OpenAPI with Versioning support (v1).

### Design Patterns
- **Repository/Service Pattern**: Logic is abstracted into services 
(`ListingService`, `BookingService`, etc.) to keep controllers lean.
- **DTOs (Data Transfer Objects)**: Decouples the internal data model from the public API schema.

---

## 📊 Data Model (ER Diagram)
<img width="458" height="783" alt="er diagram" src="https://github.com/user-attachments/assets/f797fa3e-4d3e-445a-a5ff-f3b21f5f5f3b" />





---

## 📝 Assumptions & Decisions

1. **CSV Bulk Upload**: Assumes a header format of `NoOfPeople,Country,City,Price`.
2. **Paging**: Default page size is set to 10 for listings and reports.
3. **Rate Limiting**: Per requirements, rate limiting logic is delegated to the API Gateway layer.

---

## ⚠️ Issues Encountered & Solutions

| Issue | Solution |
| :--- | :--- |
| **Transient SQL Errors** | Enabled `EnableRetryOnFailure` in the DbContext configuration to handle Azure SQL connection drops. |
| **Decimal Precision Warnings** | Explicitly configured `Price` precision in `OnModelCreating` to prevent truncation. |
| **EF Core Query Translation** | Fixed `AverageRating` logic to ensure calculations happen on the SQL server rather than client-side. |
| **Package Versioning** | Resolved conflicts between .NET 8 and .NET 10 packages by explicitly pinning versions. |

