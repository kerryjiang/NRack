NRack  [![Build Status](https://travis-ci.org/kerryjiang/NRack.svg?branch=master)](https://travis-ci.org/kerryjiang/NRack) [![NuGet Version](https://img.shields.io/nuget/v/NRack.svg?style=flat)](https://www.nuget.org/packages/NRack/)
=====

**NRack** 是一个服务器应用容器, 他能用宿主和管理你的多个后端服务。

**功能特点**:

* **自动化服务宿主**： 宿主你的多个后端程序，无需为每个程序创建服务；
* **外部程序宿主**： 宿主外部的可执行程序，可支持任何可执行程序，无论它是否是.NET开发；
* **多应用隔离**： 支持多个程序的应用程序域级别的隔离或者进程级别的隔离；
* **守护进程** ： 程序意外关闭后自动启动；
* **自动化回收管理**： 达到一定条件后自动重启程序；
* **应用程序花园(TODO)**： 为同一应用运行多个实例；
* **计划任务(TODO)**： 定时按计划运行制定程序。

文档: [http://nrack.getdocs.net/](http://nrack.getdocs.net/)