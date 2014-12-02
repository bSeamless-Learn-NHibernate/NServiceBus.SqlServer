---
title: Reusing transport connection
summary: Sample shows how NHibernate persistence can reuse SQLServer transport's connection to ensure that receive and store happen both in a same transaction without referring to a Distributed Transaction Coordinator (DTC)
tags:
- SQLServer
- NHibernate
- Transport
- DTC
---

## Problem

I would like to use NServiceBus without having to install and configure Distributed Transaction Coordinator (DTC). Can I use the same SQLServer database for both my storage and transport and configure them to reuse the same underlying ADO.NET connection and database transaction?

## Solution

Yes, NServiceBus can work on top of a SQLServer database using it both as a transport and as a persistence mechanism for timeouts and sagas. Moreover, you as a developer, can tap into the same mechanism to ensure that your data is also stored in the same transaction. Here's how you enable this feature:

 1. Use SQLServer transport

 ```
 Install-Package NServiceBus.SqlServer
 ```

 <!-- import Transport -->

 2. Use NHibernate persistence

 ```
 Install-Package NServiceBus.NHibernate
 ```

 <!-- import Persistence --> 

 3. DO NOT disable distributed transactions using `configuration.Transactions().DisableDistributedTransactions();`. While at first it might sound odd, disabling distributed transactions will force SQLServer transport to use native ADO.NET transactions. NHibernate out of the box does not allow to begin a new `ITransaction` with a given `IDbTransaction` so it will try to create a new `IDbTransaction` causing a ADO.NET exception.
 
 4. Ensure both transport and persistence connection strings are same (they cannot differ, even by case or white space, because they are used as lookup keys)
 
 <!-- import MatchingConnectionStrings -->

## Overwiew

The sample is based on a very simple order processing engine. The client is sending `NewOrder` messages to the backend

<!-- import Sender -->

The messages are handled by an `OrderLifecycleSaga` saga that pretends, using timeouts, that it processes the message.

<!-- import OrderProcessing -->

When the timeout period is over, it stores the order in the database and notifies other components that the order has been fulfilled

<!-- import NotifyFulfilled -->

NOTE: In this particular transaction there are multiple logical data stores involved, all backed up by the same physical SQLServer database:
 * A saga store that maintains saga instance data
 * An application data store that is used to persist orders
 * A queue table used by SQLServer transport to receive messages from (a timeout message)
 * A queue table used by SQLServer transport to publish messages to (an `OrderFulfilled` message)

Finally, a `OrderFulfilledHandler` is invoked

<!-- import Ship -->

NOTE: The user code fetches the order entity from the database using NHibernate session and just updates the property. The session has been transparently and lazily opened by NServiceBus and will be flushed and closed when NServiceBus finishes processing the message. 