# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**LSOmniService** is a WCF (Windows Communication Foundation) web service built on .NET Framework 4.7. It serves as an omnichannel commerce backend that bridges mobile/web clients with LS Central (LS Retail's ERP) and Dynamics Business Central, while maintaining a local SQL Server database for device, notification, and shopping list data.

## Build Commands

```bash
# Build solution (from WcfService/ directory)
msbuild LSOmniService.sln /p:Configuration=Debug /p:Platform="Any CPU"
msbuild LSOmniService.sln /p:Configuration=Release /p:Platform="Any CPU"
```

Build output goes to `Service/bin/`. The solution uses conditional compilation symbols: `TRACE;DEBUG;WCFSERVER;Swagger4WCF` (Debug) and `TRACE;WCFSERVER` (Release).

No dedicated test project exists in this solution; MSTest 3.6.2 is referenced but tests are elsewhere.

## High-Level Architecture

```
WCF Endpoints (BOService, PortalService, UCService)
    SOAP via .svc  |  JSON/REST via Json.svc
            ↓
    LSOmniBase (base class)
    - Extracts HTTP headers: LSRETAIL-KEY, LSRETAIL-TOKEN, LSRETAIL-DEVICEID, LSRETAIL-VERSION
    - Handles exceptions → WCF faults
            ↓
    Business Layer (BLL classes in BusinessLayer/)
            ↓
    ┌──────────────────────┬──────────────────────────────┐
    │  SQL Server (local)  │  BO Connector (pluggable)    │
    │  DataAccess/         │  BOConnection.CentralExt     │
    │  Data.SQLServer/     │  BOConnection.CentralPre     │
    │                      │  BOConnection.CentrAL        │
    │                      │  BOConnection.NavWS          │
    │                      │  BOConnection.NavSQL         │
    └──────────────────────┴──────────────────────────────┘
```

### Service Contracts

Three service interfaces in `Service/Interface/`:
- **IBOService** — Orders, baskets, replication, loyalty operations
- **IPortalService** — E-commerce portal / shopping cart operations
- **IUCService** — Mobile/user channel operations

Each contract has both SOAP (`.svc`) and JSON (`Json.svc`) endpoints.

### Pluggable BO Connector

The active backend connector is selected at runtime via `BOConnection.AssemblyName` in `AppSettings.config`. Options:
- `CentralExt` — LS Central v23.0+
- `CentralPre` — LS Central v17.5–23.0
- `CentrAL` — LS Central v15–17.4
- `NavWS` — Dynamics NAV via Web Services
- `NavSQL` — Dynamics NAV via direct SQL

### Data Split

- **Local SQL Server** (`SQLConnectionString.LSOmni`) — stores devices, notifications, one-lists, user config
- **LS Central / Dynamics Business Central ** — items, prices, orders, loyalty contacts, replication data

## Key Configuration Files

All live in `Service/`:
- `AppSettings.config` — DB connection strings, LS Central URL/credentials, active BO connector assembly name, license key
- `Web.config` — IIS/WCF settings, compression, ASP.NET compatibility
- `WebBindings.config` — SOAP and REST binding definitions
- `WebBehaviors.config` — WCF behavior settings (exception detail mode)
- `NLog.config` — Logging targets and rules

Critical `AppSettings.config` keys:
- `BOConnection.Nav.Url` / `BOConnection.Nav.ODataUrl`
- `BOConnection.Nav.UserName` / `BOConnection.Nav.Password`
- `BOConnection.Nav.Protocol` — `Tls12`, `S2S`, or `oAuth`
- `BOConnection.AssemblyName` — selects which connector DLL to load
- `SQLConnectionString.LSOmni`

## Project Structure

```
WcfService/
├── Service/                   # WCF host project (deployed to IIS)
│   ├── Interface/             # WCF service contracts (IBOService, IPortalService, IUCService)
│   ├── Common/                # Base service classes (LSOmniBase, LSOmniBaseLoy, etc.)
│   └── *.svc / *Json.svc      # Service endpoints
├── BusinessLayer/             # BLL classes (OrderBLL, ContactBLL, ReplicationBLL, etc.)
├── Common/                    # Shared utilities: ConfigSetting, LSNLog, Security, SQLHelper
├── DataAccess/
│   ├── Interfaces/            # IRepository interfaces
│   ├── Data.SQLServer/        # SQL Server repository implementations + SQLScripts/
│   ├── BOConnection.CentrAL/  # LS Central connector (v15–17.4)
│   ├── BOConnection.CentralExt/  # LS Central connector (v23+)
│   ├── BOConnection.CentralPre/  # LS Central connector (v17.5–23)
│   ├── BOConnection.NavCommon/
│   ├── BOConnection.NavSQL/
│   ├── BOConnection.NavWS/
│   ├── BOConnection.PreCommon/
│   └── Firebase/              # Push notification integration
└── Domain/ (referenced from parent)
    ├── Domain.DataModel.Base
    ├── Domain.DataModel.Loyalty
    ├── Domain.DataModel.Pos
    ├── Domain.DataModel.Activity
    └── Domain.DataModel.ScanPayGo
```

## Authentication

Clients authenticate via HTTP headers processed in `LSOmniBase`:
- `LSRETAIL-KEY` — service key / username
- `LSRETAIL-TOKEN` — security token
- `LSRETAIL-DEVICEID` — client device identifier
- `LSRETAIL-VERSION` — API version

Password encryption uses `Security.EncrCode` in `Common/Util/Security.cs`.

## Logging

NLog is used throughout via the `LSNLog` wrapper in `Common/Util/LSNLog.cs`. Configuration is in `Service/NLog.config`.

## External Integrations

- **Firebase** (`DataAccess/Firebase/`) — push notifications via Firebase Admin SDK
- **Braintree** — payment processing (referenced in BusinessLayer)
- **SignalR** — real-time updates (client library)
- **Google APIs** — analytics
