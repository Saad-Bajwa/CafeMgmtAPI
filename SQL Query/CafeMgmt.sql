create database CafeMgmtSystem

go

use CafeMgmtSystem

go


CREATE TABLE Users(
id int IDENTITY(1,1) PRIMARY KEY,
name varchar(250),
contactNumber varchar(20),
email varchar (50),
[password] varchar(250),
status varchar(20),
role varchar(20)
)

go


create table Category(
	id int Identity(1,1) primary key,
	name varchar(255)
)
 go
 
create table Product(
	id int identity(1,1) primary key,
	name varchar(255),
	categoryId int,
	description varchar(255),
	price int,
	status varchar(20)
)

 go


create table Bill(
	id int identity(1,1) primary key,
	uuid varchar(200),
	name varchar(255),
	email varchar(255),
	contactNo varchar(20),
	paymentMethod varchar(50),
	totalAmount int,
	productDetails nvarchar(max),
	createdBy varchar(255)
)

go

Insert Into Users(
	name,
	contactNumber,
	email,
	password,
	status,
	role
)
Values(
	'Admin',
	'1234567890',
	'admin@gmail.com',
	'admin',
	'true',
	'admin'
);

go





