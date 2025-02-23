# MedAlert - Telex ASP.NET Core Integration

## Introduction
MedAlert is a Telex integration that automates medical reminders at scheduled intervals. This integration sends timely notifications to a Telex channel based on the specified schedule in the settings.

## Features
- ✅ Sends scheduled medical reminders to a Telex channel.
- ⏳ Allows custom scheduling using CRON expressions.
- ⚙️ Supports dynamic configuration via API.

## Technologies Used
- 🟣 **C# (.NET 8)**
- 🌐 **ASP.NET Core (Web API)**
- 🔗 **Telex Integration**

## Installation

### Prerequisites
Ensure you have the following installed on your system:

- **.NET 8 SDK** – [Download Here](https://dotnet.microsoft.com/en-us/download)
- **ASP.NET Core Runtime** – Included with .NET SDK

### Steps
 Clone the Repository
```
git clone https://github.com/Qazim-tec/telex-Integration.git
cd telex-Integration

```
Install Dependencies
Run the following command inside the project directory:

```
    dotnet restore 
    dotnet build --configuration Release  
```

Run the Application

```
    dotnet run  
```

## Testing the Integration
You can manually test the integration by sending a tick request.

### Send a Reminder Every 5 Minutes
```
curl -X POST "https://telex-integration-37pm.onrender.com/api/medalert/tick" \
     -H "Content-Type: application/json" \
     -d '{
       "channel_id": "your_channel_id",
       "return_url": "your_return_url",
       "settings": [
         {
           "label": "interval",
           "type": "text",
           "required": true,
           "default": "*/5 * * * *"
         }
       ]
     }'


```




![MedAlert Preview](telex.png)