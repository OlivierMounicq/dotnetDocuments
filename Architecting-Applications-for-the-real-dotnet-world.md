# Architecting applications for the real .net world

[pluralsight.com](https://app.pluralsight.com/library/courses/architecting-applications-dotnet/table-of-contents)

## 1. Real world architectural thinking

### 1.1 Lean software development

#### 1.1.1 Lean principles

* Eliminate waste
* Amplify learning
* Decide as late as possible
* Deliver as fast as possible
* Empower the team
* Build integrity in (Automated testing)
* See the whole

#### 1.1.2 Miminum Valuable Product (MVP)

##### 1.2.2.1 MVP goals

The goal : validated learning 
* What was the response ?
* What did they like ? Hate ?
* How much profit ?
* Is it worth scaling ?
* Bad assumption ?

##### 1.2.2.2 MVP : What we don’t care about

* scalability
* maintenance cost
* and often:
  * beauty
  * code cleanliness
  * full feature set
  * performance
  * security
  
  
### 1.2 Agile architecture

Date(fast) [Missed deadline] / cost (cheap) [Blown budget] / Quality (good) [Technical debt]

### 1.3 Layers

#### 1.3.1 The layers

| Layer        | Technology                                |       |
|:-------------|:------------------------------------------|:------|
| Presentation	|	WebForms, MVC, WPF, Winforms, Silverlight	| _UI_  |
| Service		    |	Web API, WCF, ServiceStack, POCOs    			  | _API_ |
| Domain       |	C#										                              | _BLL_ |
| Data									| ADO.NET, ORM, Stored procedure            |	_DAL_ |


#### 1.3.2 Layers vs Tiers

* Layers are _logical_
* Tiers are _physical_

##### 1.3.2.1 Tiers

- scalability :  we could scale our data access layer separately from our presentation layer
- security : we could add firewall between each other of your physical tiers
- uptime : multiply the server to avoid the downtime during the maintenance
- reusable : the tiers dedicated to the database could be use by different application hosted on different tiers

__Drawbacks of the tiers__  
- performance costs
- increased complexity

##### 1.3.2.2 Layers

- separate concerns
- aid understanding
- abstract away complexity
- support testing
- minimize dependences
- enable reuse

__Drawbacks__
- leak (mix logical layers together)
- more code

## 2. Business Logic Layer

### 2.1 BBL

- center of the app
- handles logic
- manages behaviors

### 2.2 Business Layer patterns (.net)

There are 4 patterns:
- Transaction script (complexity —)
- table module 
- active record
- domain model (complexity ++)

And we can classify the pattern:
* _Procedural_ : Transaction script
* _Data-driven_ : table module / Active Record (the DB structure dictates the structure of the classes => very data centric, you can use it only for the data centric application
* _Business-driven_ : Domain Design Driven

#### 2.2.1 1<sup>st</sup> BLL approach : Transaction script

- simple
- procedural
- one public function per UI operation


| UI                     | Method                    |
|:-----------------------|:--------------------------|
| button RegisterSpeaker | public RegisterSpeaker()  |
| button SubmitOrder			  | public SubmitOrder()      |

=> handles everything 	
 
__drawbacks__
- risk of duplication
- break the Single Responsibility Principle 
- becomes painful as logic complexity grows

#### 2.2.2 2<sup>nd</sup> BLL approach : Table module

- Each class represents a table : the class is the abstraction over the a database table
- the class implements DataTable, DataSet
- the table module approach is gone out of the vogue as Entity Framework has become the most popular 	option for the DAL

__Drawbacks__
- considered a legacy approach
- this approach does tightly bind your business logic together with your data access layer.
- the structure of your DataTable will traditionally match your database schema.

#### 2.2.3 3<sup>rd</sup> BLL approach : Active Record

- each instance represents a DB row
- class knows how to persist itself
- contain business logic
- can add static methods to work on all table records
- often implemented using : Entity Framework / Linq-to-SQL / Subsonic / Castle ActiveRecord (abstraction over NHibernate)

__Pros__
- Simple and obvious
- Speedy dev and changes
- compliments CRUD app
- good for simple domain
- No OR mismatch
- Honors YAGNI

__Drawbacks__
- Rigid : Domain model = DB => if the database model changes, you have to update the business logic layer
- leads to god object (anti-pattern)
- low cohesion : break Single Responsibility Principle : by definition, this pattern mixes the data access with the business logic
- hard to test because the the BBL is highly tied to the database. It’s very difficult to inject a DB mock
- tricky transaction: each instance knows how to save itself, so it can be difficult to manage the transaction


#### 4<sup>th</sup> BLL approach : DDD

What if I designed my classes without worrying what the database looks like ?  
How would I design my object model to best solve the business problem ?  

The big idea : structure your business logic however you desire without feeling constrained about the database structure.	

For instance:
_DB Tables_ : Customer / Address / Purchase  
_Classes_ : Member (with list of addresses) / Order  

__Pros__
- Manage complexity
- Leverage design patterns
- Speak business language
- Abstract ugly DB schema
- Compliments large team
- Reusable
 
__Cons__
- learning curve
- time-consuming design
- long-term commitment
- DB mapping overhead

## 3. Service Layer

### 3.1 Goal

Service layer helps coordinate interactions among your domain objects and provides ac coarse grained API (the consumer does not need to understand the detail behind)  

### 3.2 Service Layer in a nutshell

- usually sits between the presentation and the business logic
- typically called by presentation	
- really a facade pattern (encapsulate a complicated object to wrap it up in something easier to understand)
- takes requests from one layer and sends to another
- conceptually similar to transaction script
- can centralize handling cross-cutting edge

Service Layer should be thin

### 3.3 Roles 

Core role:
- adapt data into format the representation layer requires
- delegate work to business objects

Potential roles
- security (authorization, managing user roles)
- logging 
- searching
- notification
- binding
- managing transaction
- data validation

The service layer is __like a boss__
- does not perform any task directly
- orchestrates interactions between business object
- just like boss who organizes work between people. Accomplish task through others.

The service layer is __a boundary__
- separates the presentation layer from the business layer

The service layer is __a shield__
- shields the presentation layer from business logic complexity
- supplies generic and common interface
- keeps layer loosely coupled


### 3.4 Fine vs grain coarse API

- course grained : Facade pattern
- Manages interactions among domain layer objects
- Encapsulate business logic - abstract away details
- sharable - optionally a web service  

### 3.5 When ? Where ?

When to use a service layer ?
- Multiple UIs : centralize all your logic 
- Multiple consumer of business logic
- Complex interactions among domain objects

Where is it called ?
- Services are traditionally invoked from the perception layer 
	— WebForm : the code behind
	— MVC : controller

### 3.6 Implementation and technologies

To implement the service layer, we could use different technologies:
- WCF
- WebAPI
- ServiceStack
_ POCOs (Plain Old CLR Objects) via a shared library


### 3.7 Web Service vs Shared Library

#### 3.7.1 Web Service pros

- Immediate bug fixes for all clients
- Clients can easily upgrade when desired (when versioned API offered) : we can support multiple versions of the web service simultaneously.
-  consumers can not decompile the code
- Autonomous : can scale service hardware separately (you can have a tier with just a web service )
- Tech agostic : a java client can consume a .net web service if the endpoint returns a JSON or XML response.
- enforce the operation of concerns : UI cannot “route around”
- highly scalable
- centralised deployments

#### 3.7.2 Shared library pros

- Native code called in the process : higher performance
- no serialisation overhead
- No internet connection required
- No risk of centralized service going down and impacting all consumers
- No risk of public abuse (no people gaining unauthorized access)
- Simplest thing that could possibly work : thus default to this.
    
### 3.8 What should the service layer return ?

There are 3 options:
- __Data Transfert Object__ (DTO) :  a class with no method inside
- __Copy of domain entities without behaviour__ :  no calling method with your domain 
- __Domain entity__ :  you expose your domain entity out the service layer

### 3.9 DTO

#### 3.9.1 What is a DTO ?

“Object that carries data across an application’s boundaries with the primary goal of minimising round trips”. Martin Fowler

A class with data only, no methods.  
Avoid coupling between UI and domain layers.

#### 3.9.2 When using DTO ?

1. circular reference : DTO can solve the problem when you want to save the data. For instance N user <-> N adresses : you cannot save those objects.  
2. Domain is not on same physical tier as service  
3. Domain object would mean bloated response : a DTO can provide only the properties needed by the UI or the data are too complex to be binding to UI  

### 3.10 Summary

- service layer is an intermediary
- not all applications need a service layer
- DTOs are useful but optional

## 4 Presentation Layer

### 4.1 The frameworks

Autonomous view : WebForm / WinForm
Model 2 : MVC
MVVM : WPF / Silverlight / Client-Side JS

### 4.2 Presentation Layer Architectural Goals 

- Separate data from the view : Data => M in the MVC or MVP and View => V
- support concurrent development :  a team for the View and another team for the model
- minimise the logic : maximize logic and reuse

The presentation layer should contain presentation specific logic which should be very thin. (only validation and display)

### 4.3 What belongs in the Presentation Layer ?

- User Interface (V)
- View model : the data to display and to manipulate (M) (In the WebForm, the code behind operates as the model)
-  basic data validation
- UI specific formatting
- Logic to control UI behaviours:
  * 1. Marshall data into/ out of the UI
  * 2. Hide, show, and move the components
  * 3. Capture and display errors

### 4.4 Selecting a presentation Layer

Client:
* WPF : routed UI
* Modern UI (formerly called Metro)
* WinForms

Web:
* MVC
* WebForms
* Javascript MVVM

#### 4.4.1 The WebForms

_Pros:_
- Rapid development
- Approachable
- Leverage  rich libraries

_Cons:_
- Complex abstraction
- Performance pitfalls (beware with the view state)
- tricky to test (since the constructor for the WebForm page class cannot be overridden => move as much as possible out the code behind file )

#### 4.4.2 Why MVC ?

_Pros_
- Control
- less abstraction
- separation of concerns

_Cons_
- More control => work
- learning curve 
- more files

#### 4.4.3 Why Client-Side ?

_Pros_
- rich interactivity
- declarative (the bind is declared)
- Argument server rendering
- speeding the page load

_Cons_
- require JS skills
- Deployment & errors
- no compile-time error
- another layer : YAGNI

#### 4.4.4 Server side vs Client-side rendering

_Server-side_
- Faster initial render
- Less taxing on old browser (the browser has to parse the JS => the browser hase to work to render the UI)
- Simplified SEO : the index engine can parse the complete UI code not like the JS dynamic page

_Client-side_
- Faster after render : you don’t need to use a complete post back to refresh the page
-  Rich interactions
- Reduced payload


## 5 Data Access Layer

### 5.1 What is the DAL ?

- Talks to DB
- Abstract always SQL
- Handles transaction
- Adapt object model to relational model

### 5.2 Business Layer selection impacts DAL

#### 5.2.1 The different kind of BBL

Business Layer:
- Transaction script (procedural method)  each method is triggered by the UI, it is UI-centered 
- Table module : this packages data and behavior together => this approach uses ADO.net (with DataSets and DataTable) / Table centric view
- Active Record : table row centric view
- DDD : only pattern which is truly persistent ignorant and it has no knowledge about the DAL

Finally:
* Table Module and Active Record mix business logic with data access.
* Transaction script and DDD : Data Access is a separate decision
* DDD : pure discreet business logic 

#### 5.2.3 The problem : How to move data between DataBase and .NET objects ?

=>  Map SQL results to .NET data types !

With the DDD, we have to do this : __DB Table <-> Data mapper <-> DDD objects__

#### 5.3.3 The data mapper


### 5.3 The DAL Reponsibilities

- CRUD
- Transaction : batch of work (Unit of work)
- Concurrency : manage the conflicts

_ORM handles CRUD, transaction and concurrency._

### 5.4 Object relational mismatch

#### 5.4.1 Summary

| Relational DB  | Object models  |
|:---------------|:---------------|
| Tables         | Class          |
| Columns        | Properties     |
| Rows           | Instance       |


<table>
  <tr>
    <th>Issue</th>
    <th>DataBase</th>
    <th>Object Model </th>
  </tr>
  <tr>
    <td valign="top">Inheritance</td>
    <td valign="top">Table per class <br/>Table per concrete class <br/> Table per class family</td>
    <td valign="top">Native support</td>
  </tr>
  <tr>
    <td valign="top">Associations</td>
    <td valign="top">Associatee to Associator</td>
    <td valign="top">Associator to Associatee</td>
  </tr>
  <tr>
    <td valign="top">Identity</td>
    <td valign="top">Key required: Identical row is considered corruption</td>
    <td valign="top">Location in memory</td>
  </tr>
  <tr>
    <td valign="top">Queries</td>
    <td valign="top">SQL</td>
    <td valign="top">Query-By-Example<br/>Query-by-API<br/>Query-by-Language (LINQ)</td>
  </tr>
  <tr>
    <td valign="top">Partial results</td>
    <td valign="top">No problems. Select desired columns</td>
    <td valign="top">1. Allow nullable fields <br> Ignore domain restriction <br/>2. Always return fully hydrated object <br/>3. Lazy loaded fields</td>
  </tr>  
</table>

#### 5.4.2 Options to deal with object relational mismatch

1. No object : write procedural code
2. Store objects in DB : db4o or NoSQL (and you could inject the information from the NoSQL to a relational DB)
3. Use relational model in code : e.g. Table Module or Active Record
4. Use ORM 
5. Manual mapping : use raw ADO.NET to map by hand

### 5.5 The DAL patterns

#### 5.5.1 ORM (Object Relational Mapping)

##### 5.5.1.1 Properties

- Change tracking : track changes and generate corresponding SQL
- Identity map : get object from memory if available to assure single instance and avoid DB call
- Lazy-loading : get data just-in time
- eager fetching : load related data automatically
- cascades: cascade related change
- Unit of work tracking : track objects in a transaction, coordinate writing changes and resolve concurrency issues

##### 5.5.1.2 ORM honors DRY

For instance, if I add a column in a table in the database, you have to do :
1. DataBase
2. Class property
3. Select statements
4. Insert statements
5. Update statements
6. Delete statements
7. Custom marshalling code (the code which manages the data exchange between the database and the application)

But if you use NHibertnate, you will have to do:
1. Update database
2. Class property
3. update .hbm.xml file

##### 5.5.1.3 Should I ORM ?

_Pros_
* Faster development
* RDMS agnostic
* Typesafe interface
* avoid writing SQL
* SQL injection protection
* Security, performance, caching, mapping Out Of The BOX (OOTB)
* Documentation

_Cons_:
* performance penalty
* learning curve
* trusting third party with SQL (the SQL generation)
* Do you own the schema ?
* DBAs lose the control (on the SQL which is generated)
* Leaky abstraction : very difficult to determine the root cause
* Security concerns (in finance, generally, the developer cannot write in the database so they won't use the ORM power)

#### 5.5.2 Repository pattern

Repository pattern = clean API

##### 5.2.2.1 Properties

- Hides data access behind an interface
- Isolates domain objects from the database access code
- Object-oriented view of the persistence layer
- Groups common queries
- Centralizes query construction
- Encapsulates he object persisted in a data store and the operations performed over them

##### 5.2.2.2 Coarse grainded

With the _repository pattern_, the DAL act as Service Layer by aggregating the common commands.

##### 5.2.2.3 Why hide the DB behind an interface ?

1. Unit testing
  * you can avoid to hit the database during the unit testing
  * you could also use a in-memory database to enhance performances of the unit tests.
2. Database independance (regarding the BLL)
3. Abstract complexity of multiple data sources (Database, file, ...)

##### 5.2.2.4 Pros/Cons

_Pros_:
- Total persistence ignorance
- Clear separation of concerns
- Swap DB technologies
- Abstract ultiple data sources
- Can completely switch DB paradigms
- Support unit testing : it is very easy to mock up the database by using an in-memory version of the database
-Support concurrent developments
- Avoids duplicate queries
- Strict control over queries
- Object oriented API
- Centralizes access rules
- Caching (and can centralizes it)

_Cons_
- Reduces ORM power
- API explosion risk : to many methods (GetUserByFirstName, GetUserByLastNameDescending...)
- Easy to leak persistence specific info if you are not careful => __how coarse grained to make your API ?__
- Yet another layer = more work

An example to reduce the methods exposed by the API

```cs
public interface IUserRepository
{
    void Add(User newUser);
    void Remove(User user);
    IQueryable<User> Find(Expression<Func<User, bool>> predicate);
}

```

##### 5.2.2.3 ORM behind a Repository? Is that nuts ?

Not necessarily. Different purposes.  

__Repository__  
Abstract and encapsulate _everything_ storage related.  
Architectural pattern

__ORM__  
Abstract access to any supported _relational database_  
When behind a repository, simply an implementation detail

=> You can use either. Or both.

#### 5.5.3 Stored procedure

##### 5.5.3.1 Pros/Cons
_Pros_:
- Avoid multiple calls to the DB
_ Help protect from SQL injection (but ORMs or ADO.NET do)
- Can place query generation/tweaks in hands of DBA
- Limit data access


_Cons_:
- Vendor lock-in : The Oracle stored procedure cannot run in the SQL Server database
- Stored procedure are not typically faster
- Risk for business logic to slip in. Code in BLL is :
    - Easier to enhance and maintain
    - Easier to test
    - Easier to debug

##### 5.5.3.2 Stored Procedure or not

Question to ask:
- How much work is it to change a Stored Procedure vs code in your organization ?
- Is your database under source control ? The Stored Procedure must be saved in a source control
- How automated is your deployment process ?
- __Risk__ : Are we placing business logic in Stored Procedure ? Separate concerns











-----------------------------------------------------------------------------------------------------------

### 6.x Feature comparison

| Feature                        | Level 1 | Level 2 | Level 3 |
|:-------------------------------|:--------|:--------|:--------|
|Centralized Data Access         |         |    ?    |    X    |
| Mockable Data Access           |         |    ?    |    X    |
| Central Lifetime management    |         |    ?    |    X    |
| Separation of concerns         |         |    ?    |    X    |
| Domain-Driven Design           |         |    ?    |    X    |
| Unit testing friendly          |         |    ?    |    X    |
| Large team friendly            |         |    ?    |    X    |
| Service Oriented               |         |    ?    |    X    |
| Honors SOLID principles        |         |    ?    |    X    |
| Single-page App (SAP) friendly |         |    ?    |    X    |
| Sinplest thing / YAGNI         |     X   |    ?    |         |



### 6.y Moving to level 2 : focus on the pain

| Pain                                        | Potential solution             |
|:--------------------------------------------|:-------------------------------|
| Redundant queries / DML                     |  Repository pattern            |
| Inability to unit test without hitting DB   | Repository pattern             |
| Difficulty styling erver-side controls      | MVC, KO, Angular               |
| Excessive postbacks to support response UI  | KO, Angular                    |
| Fat controller / Redundance BL Interactions | Service Layer                  |
| Other clients desire service                | SOA or shared library          |
| Data access performance                     | Micro ORM (Dapper, ORMLite...) |
| Ugly data schema / Multiple Datasources     | Repository Pattern             |
| Complex business logic                      | DDD                            |
