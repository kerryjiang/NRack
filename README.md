NRack  [![Build Status](https://travis-ci.org/kerryjiang/NRack.svg?branch=master)](https://travis-ci.org/kerryjiang/NRack) [![NuGet Version](https://img.shields.io/nuget/v/NRack.svg?style=flat)](https://www.nuget.org/packages/NRack/)
=====

**NRack** is a server application container, which can be used for your back end services' hosting and management.

**Features**:

* **Automatic service hosting**: host your many backend applications, needn't create service for each application;
* **External application hosting**: host external executable application, no matter whether it is developed using .NET;
* **Multiple application isolation**: support isolate your multiple applications using AppDomain and process;
* **Watch dog**: start application after it stops unexpected;
* **Automatic recycle management**: restart application automatically after it satisify some conditions;
* **App Garden(TODO)**: run one application in multiple instances;
* **Task Scheduler(TODO)**： run the designate application on schedule.

Documentation: [http://nrack.getdocs.net/](http://nrack.getdocs.net/)
