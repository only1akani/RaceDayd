CREATE DATABASE RaceDaydb;

USE RaceDaydb;

CREATE TABLE Roles(
role_id int identity(1,1) primary key,
role_name varchar(50) not null
);

CREATE TABLE Users(
user_id int identity(1,1) primary key,
role_id int not null,
full_name varchar(100) not null,
email varchar(100) not null,
password_hash varchar(255) not null,
id_number varchar(20) not null,
created_at datetime not null default getdate(),
constraint FK_Users_Roles foreign key(role_id) references Roles(role_id)
);

CREATE TABLE Events(
event_id int identity(1,1) primary key,
organiser_id int not null,
event_name varchar(100) not null,
event_description varchar(500),
event_date datetime not null default getdate(),
location varchar(150),
constraint fk_events_users foreign key(organiser_id) references Users(user_id)
);

CREATE TABLE categories(
category_id int identity primary key,
event_id int not null,
category_name varchar(50),
distance decimal(5,2) not null,
price decimal(8,2) not null default 0,
max_participants int not null default 100,
constraint FK_Categories_Events foreign key(event_id) references Events(event_id)
);

CREATE TABLE Enrolments(
enrolment_id int identity(1,1) primary key,
participant_id int not null,
category_id int not null,
enrolment_date datetime not null default getdate(),
status varchar(20) not null default 'Pending',
constraint FK_Enrolments_Categories foreign key(category_id) references categories(category_id),
constraint FK_Enrolments_Users foreign key(participant_id) references Users(user_id)
);

CREATE TABLE Results(
result_id int identity(1,1) primary key,
enrolment_id int not null,
finish_time time null,
position int null,
recorded_at datetime not null default getdate(),
constraint FK_Results_Enrolments foreign key(enrolment_id) references Enrolments(enrolment_id)
);

insert into Roles(role_name) values ('Organiser');
insert into Roles(role_name) values ('Participant');

insert into Users (role_id, full_name, email, password_hash, id_number)
values 
(1, 'Thabo Nkosi', 'thabo.nkosi@raceday.co.za', 'HASHED_PASSWORD_1', '9001015800089'),
(1, 'Lerato Dlamini', 'lerato.dlamini@raceday.co.za', 'HASHED_PASSWORD_2', '8805124800088'),
(2, 'Sipho Khumalo', 'sipho.khumalo@gmail.com', 'HASHED_PASSWORD_3', '9503215800081'),
(2, 'Ayesha Patel', 'ayesha.patel@gmail.com', 'HASHED_PASSWORD_4', '9711065800082'),
(2, 'Johan van der Merwe', 'johan.vdm@gmail.com', 'HASHED_PASSWORD_5', '9209015800083');

insert into Events (organiser_id, event_name, event_description, event_date, location)
values (1, 'Joburg City Marathon', 'Annual road marathon through the streets of Johannesburg.', '2026-11-14 06:00:00', 'Johannesburg, Gauteng');
insert into Events (organiser_id, event_name, event_description, event_date, location)
values (1, 'Soweto Fun Run', 'Community fun run for all ages and fitness levels.', '2026-09-27 07:00:00', 'Soweto, Gauteng');
insert into Events (organiser_id, event_name, event_description, event_date, location)
values (2, 'Cape Town Cycle Challenge', 'Scenic cycling event around the Cape Peninsula.', '2026-10-10 06:30:00', 'Cape Town, Western Cape');

insert into categories (event_id, category_name, distance, price, max_participants)
values 
(1, '10km Run', 10.00, 150.00, 500),
(1, '21km Half Marathon', 21.10, 250.00, 300),
(1, '42km Full Marathon', 42.20, 350.00, 200),
(2, '5km Fun Run', 5.00, 80.00, 1000),
(2, '10km Run', 10.00, 120.00, 500),
(3, '60km Cycle', 60.00, 400.00, 400),
(3, '100km Cycle', 100.00, 550.00, 250);

insert into Enrolments (participant_id, category_id, status)
values
(3, 1, 'Confirmed'),
(4, 2, 'Confirmed'),
(5, 4, 'Pending'),
(3, 6, 'Confirmed');

insert into Results (enrolment_id, finish_time, position)
values
(1, '00:48:32', 12),
(2, '01:52:07', 5);

select * from Roles;
select * from Users;
select * from Events;
select * from categories;
select * from Enrolments;
select * from Results;