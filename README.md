RaceDay



RaceDay is a web-based event management system for the South African road running, walking and cycling community. This repository holds all three parts of the project. Part 2 (the REST API) is in the `Part2` folder.



\## Part 2: RESTful API



\*\*YouTube video (unlisted):https://youtu.be/5CLidgMO5wQ



\*\*GitHub repository:\*\* https://github.com/only1akani/RaceDayd



The API is built with ASP.NET Core Web API (.NET 8), Entity Framework Core (Code-First) and SQL Server. It follows the endpoint plan from Part 1, with the changes listed under "Deviations from Part 1".



\### The two roles



\*\*Organiser\*\*

\- Creates, edits and deletes their own events

\- Adds, edits and deletes categories for their own events

\- Views all enrolments for their own events

\- Captures finish times and positions for participants in their own events

\- Cannot enrol in events



\*\*Participant\*\*

\- Enrols in an event by choosing a category

\- Views and cancels their own enrolments

\- Views their own results

\- Cannot create events, manage categories or capture results



Both roles can register, log in, log out, and view and update their own profile. Browsing events, categories and event results is public. A role is chosen at registration (Organiser or Participant). The API checks it against the Roles table and rejects anything else with a 400.



\### Authentication



\- Passwords are hashed with BCrypt before being saved. The real password is never stored.

\- On login the server stores the user id and role in a server-side session (cookie based).

\- A custom `\[SessionAuthorize]` filter protects every non-public endpoint. It returns 401 if nobody is logged in and 403 if the role is wrong.

\- Ownership is checked inside each action. An organiser can only change their own events, and a participant can only cancel their own enrolment.



\### Setup and run instructions



\*\*Requirements:\*\* .NET 8 SDK (or newer), SQL Server LocalDB or SQL Server, and the EF Core tool (`dotnet tool install --global dotnet-ef`).



1\. Clone the repository:

```

&#x20;  git clone https://github.com/only1akani/RaceDayd.git

&#x20;  cd RaceDayd/Part2

```

2\. Open `RaceDay.Api/appsettings.json` and check the connection string. It defaults to a LocalDB instance called `api\_guide`:

```

&#x20;  Server=(localdb)\\\\api\_guide;Database=RaceDaydb;Trusted\_Connection=True;TrustServerCertificate=True;

```

&#x20;  Change the server name if your instance is different. To create the instance, run `sqllocaldb create api\_guide` and then `sqllocaldb start api\_guide`.

3\. Create the database from the migration (this also seeds the two roles):

```

&#x20;  dotnet ef database update --project RaceDay.Api

```

4\. Run the API:

```

&#x20;  dotnet run --project RaceDay.Api --launch-profile https

```

5\. Open the Swagger page at `https://localhost:7262/swagger` (use the port shown in the console).

6\. Register an Organiser and a Participant with `POST /api/auth/register`, then log in with `POST /api/auth/login`. The session cookie is kept by the browser, so every other endpoint can then be tried directly in Swagger.



\### Endpoints



| Method | Route | Role | Purpose |

|---|---|---|---|

| POST | /api/auth/register | Public | Register as an Organiser or Participant |

| POST | /api/auth/login | Public | Log in and start a session |

| POST | /api/auth/logout | Logged in | End the session |

| GET | /api/users/{id} | Logged in (own) | View own profile |

| PUT | /api/users/{id} | Logged in (own) | Update own profile |

| GET | /api/events | Public | List all events |

| GET | /api/events/{id} | Public | Event details with categories |

| POST | /api/events | Organiser | Create an event |

| PUT | /api/events/{id} | Organiser (owner) | Edit an event |

| DELETE | /api/events/{id} | Organiser (owner) | Delete an event |

| GET | /api/events/{eventId}/categories | Public | List an event's categories |

| POST | /api/events/{eventId}/categories | Organiser (owner) | Add a category |

| PUT | /api/categories/{id} | Organiser (owner) | Edit a category |

| DELETE | /api/categories/{id} | Organiser (owner) | Delete a category |

| POST | /api/categories/{categoryId}/enrolments | Participant | Enrol in a category (status Pending) |

| GET | /api/enrolments/my | Participant | View own enrolments |

| GET | /api/events/{eventId}/enrolments | Organiser (owner) | View all enrolments for an event |

| DELETE | /api/enrolments/{id} | Participant (owner) | Cancel own enrolment |

| POST | /api/enrolments/{enrolmentId}/results | Organiser (owner) | Capture a finish time and position |

| GET | /api/results/my | Participant | View own results |

| GET | /api/events/{eventId}/results | Public | View an event's results |



Every endpoint is described in Swagger, including the data it expects and the responses it returns.



\### Unit tests



The `RaceDay.Tests` project (xUnit) starts the real API with an in-memory database and covers expected successes and failures for:

\- User registration and login

\- Event management by organisers

\- Role-based access (wrong role gives 403, no login gives 401)

\- Event enrolments, including checking the enrolment is recorded in the database



Run them with:

```

dotnet test RaceDay.sln

```



\### CI/CD



A GitHub Actions workflow (`.github/workflows/part2.yml`) builds the project and runs all the unit tests on every push.



!\[Green build](Part2/images/ci-green.png)



\### Deviations from Part 1



\- \*\*Session instead of JWT.\*\* The Part 1 plan used a JWT token. The Part 2 brief requires session management, so login now stores the user id and role in a server-side session.

\- \*\*Logout endpoint added.\*\* Sessions need a way to end, so `POST /api/auth/logout` was added.

\- \*\*Full categories.\*\* Enrolling in a category that has reached its `max\_participants` returns 409.

\- \*\*SQL script and ERD aligned.\*\* The unique constraints on `Users.email`, `Results.enrolment\_id` and `Enrolments(participant\_id, category\_id)` and the `created\_at` column on Events were added to `docs/Raceday.sql` so it matches the ERD and the EF Core model.

\- \*\*Cascade deletes.\*\* Deleting an event also deletes its categories, enrolments and results.



\---



\## Part 1: System Planning and Database



\*\*YouTube video:\*\* https://youtu.be/JAXZWTTiL6o



Part 1 was planning only. It produced the ERD, the API endpoint plan and the SQL script, all in the `docs` folder.

