# MedCare Platform

Enterprise Hospital Management System (HMS) built on .NET Clean Architecture, Dapper, SQL Server, and Angular.

## Architecture Highlights
- 9 Domain-Driven Bounded Context Schemas (`core`, `party`, `clinical`, `diagnostic`, `inventory`, `billing`, `finance`, `security`, `audit`)
- Clean Architecture (Domain, Application, Infrastructure, Presentation API)
- High-performance Dapper ORM with Table-Valued Parameters (TVPs)
- Angular 18+ Single Page Application