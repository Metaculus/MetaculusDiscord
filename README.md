# Metaculus Discord Bot
- unofficial Discord bot for [Metaculus](https://www.metaculus.com)
- to be released soon!


## Features
- [x] Searching the questions, embedding the official chart
- [x] DM alerts for questions
- [x] Channel alerts for questions
- [x] Following categories in a channels to get notified of new questions, updates and resolutions

## User documentation
You can add the bot to your server with this [~~invite link~~](https://www.youtube.com/watch?v=dQw4w9WgXcQ)
### Regular users 
- use `/metaculus <query>` to search for questions
  - The bot's response is decorated with reaction emojis 1-5, that when clicked will post the question link, causing an embed with a chart to appear.
- put a `:warning:` emoji ⚠️ on a question link to get notified of its updates to DMs
  - removing the emoji removes the alert

- `!metac help`: displays these instructions

 
### Commands mainly for moderators
When the bot is running it is listening for *commands* prefixed by `!metac`, though this is not the intended usage for regular users.

- `!metac alert <question_id>` sets a resolution and swing alert for the question sent to DMs
- `!metac unalert <question_id>` removes the alert for the question

 
- `!metac channelalert <question_id>` 
- `!metac unchannelalert <question_id>` removes the alert for the question


- `!metac listcategories` to list all categories
- `!metac followcategory <category_id>` to follow a category in this channel
- `!metac unfollowcategory <category_id>` to unfollow a category


- `!metac s[earch] <query>`: returns 5 best matches for the query (same as /metaculus)

## Developer documentation

### Running the bot
The bot is not intended to be run outside of a single production, running more instances can interfere with the slash command functionality.
It uses PostgreSQL as its database.

If for some reason you want to run the bot, do this:
1. Install **dotnet6.0** and **PostgreSQL**
2. Create a valid *appsettings.json* from the *appsettings_template.json*
3. Install dependencies with `dotnet restore` in the project directory
4. Install **Entity Framework** tools `dotnet tool install --global dotnet-ef`
5. Create database migration `dotnet ef migrations add <migration_name>`
6. Apply migration to database `dotnet ef database update <migration_name>`
7. Run the bot with `dotnet run`

The repo also contains a Dockerfile that will run the bot (though you'll still need to setup the database and `appsettings.json`).

### The purpose and the problems: 
1. People don't have time to check forecasts on Metaculus, but they use discord daily.
2. The most important thing for people is to know when a question has resolved or shifted significantly, therefore the bot's main purpose is to send these alerts.
3. People don't like to use regular commands. So the main user interaction should be through the pleasant slash command API and emoji. 
4. For moderators it's annoying to set up each question alert manually so there is an option to set categorical alerts.

### Design 
The underlying library is [Discord.NET](https://discordnet.dev/index.html). The bot is composed of several services, modules and a Data access layer. 
The services are separated, if one has an error, the others keep their functionality. I used Entity Framework Core to simplify database creation.

### The insides
#### Data
The class **Data** implements generic CRD methods for alerts in the PostgreSQL database. 
Entity Framework is used so that the database schema can be generated automatically. 
Each request for the database has its own **MetaculusContext** which is created by a **MetaculusContextFactory**.
It is injected to all services and modules that work with the database (almost all).
#### Model
The most contrived part of the bot are the Model classes, because they parse the Metaculus API which is not very friendly to C# as it does not have a static schema.

In *Alert.cs* there is a type hierarchy of classes for the alerts that are put into the database annotated for the purpose of Entity Framework. The sealed **UserQuestionAlert, ChannelQuestionAlert, ChannelCategoryAlert** are the ones in the database.

**Category** is a class that holds noteworthy (should be alerted) questions from a category.  

In *Question.cs* there is the **Question** base class but mainly 
**SearchResultQuestion** that is used to parse when we want to display only base data about the question. 
**AlertQuestion** is used to parse the question for the purposes of the alerts, which means it has to extract predictions at a certain point of time.

**SearchResponse** is used as a container of SearchResultQuestions that are to be displayed after search. In its file there is also **ResponseLinks** that is used to store full links for the purpose of when a question from search is selected then it would have a nicer link.

#### Services
**AlertService** abstract class that contains methods that can create and send messages from alerts and parsed questions.

**QuestionAlertService** every 6 hours checks all singular question alerts and sends updates to DMs or channels.

**CategoryAlertService** every 24 hours checks all category follows and sends updates the respective channels. 
It does this by creating a **Category** class for each unique category, joining it with category alert and sending messages for question updates (new, prediction swing, resolved).

**InteractionHandler** Registers command modules, handles registered messages, that get converted to commands in case they have the prefix. Receives an event when emoji is updated, that this then used to send a message or update database based on it.

#### Modules
Modules are classes without state, that only know what their context is (so that they know how to reply), with only command and utility methods.

**Bot(Interaction)ModuleBase** extends the Discord.Net module bases and adds *Data* to them. They are superclasses of all the modules that need the database.

**AlertCommands** Commands for manipulating single question alerts.

**FollowCommands** Commands for (un)following categories in a channel.

In the *Search.cs* file there are two modules with the same purpose but different medium, **SearchSlash** utilizes slash command API, and **SearchCommands** uses normal message commands. They both search for questions and return the top 5 results, saving their links to Data to be used later.

**UtilCommands** `help` and `listcategories`.

#### Utils
Static classes for things needed throughout the bot.

**ApiUtils** for downloading a question from the Metaculus API. And validating categories for following.

**EmotesUtils** Holds numeric emojis for the reactions on the question links.


## Final remarks
All bugs should be attributed to me and not Metaculus, all the good things Metaculus does should be attributed to Metaculus and not me. 
PRs are welcome, but if you fork and run the bot, please don't add it to a server that already contains this one.
It should be simple to modify the app to use a different database (just search and replace .UseNpgsql in the code).
