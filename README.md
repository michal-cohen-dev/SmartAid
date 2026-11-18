# SmartAid – Smart Roadside Assistance Platform

SmartAid is a web-based platform that connects people who need roadside assistance with nearby volunteers who can help them.

The system uses AI to analyze reported problems and a smart matching algorithm to find suitable volunteers based on their location, availability, required skills, and equipment.

## Key Features

- **AI-Powered Analysis** – Analyzes text descriptions and images of roadside problems using Google Gemini.
- **Smart Volunteer Matching** – Matches requests with eligible volunteers using distance and reliability scores.
- **Real-Time Notifications** – Notifies volunteers about relevant requests using SignalR and email.
- **Request Management** – Allows users to create, track, and manage assistance requests.
- **Secure Authentication** – Uses JWT-based authentication and role-based authorization.

## Technologies

**Frontend:** React, TypeScript, React Router

**Backend:** C#, ASP.NET Core Web API, Entity Framework Core

**Database:** SQL Server

**External Services:** Google Gemini API, TomTom API, SMTP

## Architecture

The project follows a layered architecture:

React Client → API Controllers → Business Logic Layer (BLL) → Data Access Layer (DAL) → SQL Server

This separation improves maintainability, flexibility, and testability.

---

**SmartAid – Connecting people through technology and community support.**
