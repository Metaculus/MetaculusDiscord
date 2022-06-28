# Metaculus Discord Bot
- unofficial Discord bot for [Metaculus](https://www.metaculus.com)
- in development, to be released soon!


## Features
- [x] searching the questions, embedding the official chart
- [x] DM alerts for questions
- [x] channel alerts for questions
- [x] following categories in a channels to get notified of new questions, updates and resolutions

## User documentation
You can add the bot to your server with this [invite link]("https://not released yet")
### Regular users 
- use `/metaculus <query>` to search for questions
- put a `:warning:` emoji ⚠️ on a question link to get notified of its updates to DMs
  - removing the emoji removes the alert

### Commands mainly for moderators
When the bot is running it is listening for *commands* prefixed by `!metac`, though this is not the intended usage.

- `!mc help`: displays these instructions
 
- `!mc s[earch] <query>`: returns 5 best matches for the query (same as /metaculus)

The bot's response is decorated with reaction emojis 1-5,  that when clicked by users will post the question link, causing an embed with a chart to appear.

- `!mc alert <question_id>` sets a resolution and swing alert for the question sent to DMs
- `!mc unalert <question_id>` removes the alert for the question
 
- `!mc channelalert <question_id>` 
- `!mc unchannelalert <question_id>` removes the alert for the question




## Developer documentation

### Running the bot
The bot is a singleton so it's not intended to be run outside of a single production, running more instances can interfere with the slash command functionality.
It uses PostgreSQL as its database.

If for some reason you want to run the bot, do this:
1. Install **dotnet6.0** and **PostgreSQL**
2. Create a valid *appsettings.json* from the *appsettings_template.json*
3. Install dependencies with `dotnet restore` in the project directory
4. Install **Entity Framework** tools `dotnet tool install --global dotnet-ef`
5. Create database migration `dotnet ef migrations add <migration_name>`
6. Apply migration to database `dotnet ef database update <migration_name>`
7. Run the bot with `dotnet run`

The repo also contains a Dockerfile that should be able to run the bot in a container (though you'll still need to setup the database).

### General design
The underlying library is [Discord.NET](https://discordnet.dev/index.html). The bot is composed of several services, modules and a Data Access Layer. 
#### Data
The data access layer uses a DbContext to access the PostgreSQL database. 
Entity Framework is used so that the database schema can be generated automatically. 
Each request for the database has its own **MetaculusContext** which is created by a **MetaculusContextFactory**.

#### Services
**AlertService** abstract class that

**QuestionAlertService** every 6 hours checks all singular question alerts and sends updates to DMs or channels.

**CategoryAlertService** every 24 hours checks all category follows and sends updates the respective channels.

**InteractionHandler** handles all commands and emoji changes and distributes them to modules.