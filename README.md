# CREEOT — Influencer Marketplace Platform

**CREEOT** is a full-stack influencer marketing platform that connects brands with content creators through multi-criteria discovery, campaign management, and escrow-protected payment workflows.

## Key Features
* **Brand & Creator Portals:** Role-based onboarding and dashboards powered by ASP.NET Core Identity.
* **Creator Discovery:** Dynamic filtering by target market, social platform, niche category, and budget range with localized currency support.
* **Campaign & Escrow Management:** End-to-end campaign assignment and escrow deposit tracking (Cards, Net Banking, UPI).
* **API & OData Layer:** RESTful endpoints with OData query support (`$filter`, `$select`, `$orderby`) and transactional email dispatch via MailKit.

---

## Repository Structure

```text
CREEOT/
├── CREEOT.sln              # Visual Studio Solution
├── CREEOT.WEB/             # ASP.NET Core MVC & Razor Pages frontend application
├── CREEOT.API/             # ASP.NET Core Web API & OData backend services
├── Database/               # MS SQL Server schema, tables, and seed scripts
│   ├── 01_Schema.sql
│   ├── 02_Tables.sql
│   └── 03_SeedData.sql
├── .gitignore
└── README.md
