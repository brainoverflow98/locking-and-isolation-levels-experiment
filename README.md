I was confused by the answers given in SO due to them being partial answers and not providing the big picture. I've made this experiment and saw that my understanding of locking and isolation levels were not complete. After I read couple documentation pages from different RDMS providers these are my takes:

TRANSACTIONS
----------------

Statements are database commands mainly to read and modify the data in the database. Transactions are scope of single or multiple statement executions. They provide two things:

 1. A mechanism which guaranties that all statements in a transaction are executed correctly or in case of a single error any data modified by those statements will be reverted to its last correct state (i.e. rollback). What this mechanism provides is called **atomicity**.
 2. A mechanism which guaranties that concurrent read statements can view the data without the occurrence of some or all phenomena described below. 

> **Dirty read:** A transaction reads data written by a concurrent
> uncommitted transaction.
> 
> **Nonrepeatable read:** A transaction re-reads data it has previously read
> and finds that data has been modified by another transaction (that
> committed since the initial read).
> 
> **Phantom read:** A transaction re-executes a query returning a set of
> rows that satisfy a search condition and finds that the set of rows
> satisfying the condition has changed due to another recently-committed
> transaction.
> 
> **Serialization anomaly:** The result of successfully committing a group
> of transactions is inconsistent with all possible orderings of running
> those transactions one at a time.

What this mechanism provides is called **isolation** and the mechanism which lets the statements to chose which phenomena should not occur in a transaction is called **isolation levels**.

As an example this is the isolation-level / phenomena table for PostgreSQL:
[![enter image description here][1]][1]


If any of the described promises is broken by the database system, changes are rolled back and the caller notified about it.


How these mechanisms are implemented to provide these guaranties is described below.

LOCK TYPES
-----

 1. **Exclusive Locks:** When an exclusive lock acquired over a resource no other exclusive lock can be acquired over that resource. Exclusive locks are always acquired before a modify statement (INSERT, UPDATE or DELETE) and they are released after the transaction is finished. To explicitly acquire exclusive locks before a modify statement you can use hints like FOR UPDATE(PostgreSQL, MySQL) or UPDLOCK (T-SQL).
 2. **Shared Locks:** Multiple shared locks can be acquired over a resource. However, shared locks and exclusive locks can not be acquired at the same time over a resource. Shared locks might or might not be acquired before a read statement (SELECT, JOIN) based on database implementation of isolation levels.

LOCK RESOURCE RANGES
-----------

 1. **Row:** single row the statements executes on.
 2. **Range:** a specific range based on the condition given in the statement (SELECT ... WHERE).
 3. **Table:** whole table. (Mostly used to prevent deadlocks on big statements like batch update.)

As an example the default shared lock behavior of different isolation levels for SQL-Server :
[![enter image description here][2]][2]

DEADLOCKS
---------
One of the downsides of locking mechanism is deadlocks. A deadlock occurs when a statement enters a waiting state because a requested resource is held by another waiting statement, which in turn is waiting for another resource held by another waiting statement. In such case database system detects the deadlock and terminates one of the transactions. Careless use of locks can increase the chance of deadlocks however they can occur even without human error.

SNAPSHOTS (DATA VERSIONING)
----------------------------
This is a isolation mechanism which provides to a statement a copy of the data taken at a specific time. 

 1. **Statement beginning:** provides data copy to the statement taken at the beginning of the statement execution. It also helps for the rollback mechanism by keeping this data until transaction is finished.

 2. **Transaction beginning:** provides data copy to the statement taken at the beginning of the transaction.


All of those mechanisms together provide ***consistency***.

When it comes to Optimistic and Pessimistic locks, they are just namings for the classification of approaches to concurrency problem.

> **Pessimistic concurrency control:**
> 
> A system of locks prevents users from modifying data in a way that
> affects other users. After a user performs an action that causes a
> lock to be applied, other users cannot perform actions that would
> conflict with the lock until the owner releases it. This is called
> pessimistic control because it is mainly used in environments where
> there is high contention for data, where the cost of protecting data
> with locks is less than the cost of rolling back transactions if
> concurrency conflicts occur.
> 
> **Optimistic concurrency control:**
> 
> In optimistic concurrency control, users do not lock data when they
> read it. When a user updates data, the system checks to see if another
> user changed the data after it was read. If another user updated the
> data, an error is raised. Typically, the user receiving the error
> rolls back the transaction and starts over. This is called optimistic
> because it is mainly used in environments where there is low
> contention for data, and where the cost of occasionally rolling back a
> transaction is lower than the cost of locking data when read.


For example by default PostgreSQL uses snapshots to make sure the read data didn't change and rolls back if it changed which is an optimistic approach.  However, SQL-Server use read locks by default to provide these promises.

The implementation details might change according to database system you chose. However, according to database standards they need to provide those stated transaction guarantees in one way or another using these mechanisms. If you want to know more about the topic or about a specific implementation details below are some useful links for you.

 1. [SQL-Server - Transaction Locking and Row Versioning Guide][3]
 2. [PostgreSQL - Transaction Isolation][4]
 3. [PostgreSQL - Explicit Locking][5]
 4. [MySQL - Consistent Nonlocking Reads][6]
 5. [MySQL - Locking][7]
 6. [Understanding Isolation Levels (Video)][8]


 


  [1]: https://i.stack.imgur.com/OiGV5.png
  [2]: https://i.stack.imgur.com/ePMkf.png
  [3]: https://docs.microsoft.com/en-us/sql/relational-databases/sql-server-transaction-locking-and-row-versioning-guide?view=sql-server-ver15#Basics
  [4]: https://www.postgresql.org/docs/current/transaction-iso.html
  [5]: https://www.postgresql.org/docs/9.1/explicit-locking.htm
  [6]: https://dev.mysql.com/doc/refman/8.0/en/innodb-consistent-read.html
  [7]: https://dev.mysql.com/doc/refman/8.0/en/innodb-locking.html
  [8]: https://youtu.be/-gxyut1VLcs
