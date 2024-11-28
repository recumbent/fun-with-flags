---
marp: true
paginate: true
footer: @murph.recumbent.co.uk -.NET Meetup Northeast - 2024-11-27 
---

# Hello!

* In 1983 I was starting the year in which I wrote a compiler like things using YACC and LEX...
* And I wrote a recursive descent parser too... 
* ..in Pascal 😜

---

# Fun With Flags
![Picture of Sheldon Cooper presenting Fun With Flags](sheldon-cooper-presents-fun-with-flags-1280w.jpg)

---

# What problem are we trying to solve?

<!--
There is strong evidence to suggest that the best way to work is trunk driven development - enough smart people being successful and with reasonable evidence - DORA metrics - to suggest its certainly something we should look at.
-->

---

# Trunk Based Development

* Continuous Integration
* Continuous Delivery (if not deployment)

<!--
To make this work we have to do proper continuous integration - pushing our code to the trunk (not to a branch, but to the main branch we build and deploy from) at least once a day.

Inherent in this is the assumption that if you push code it _will_ go into production (that leads to continuous delivery and contninuous deployment)
-->

---

# Implies

* Automated Tests (high confidence)
* No long lived feature branches
* Push _at least_ daily...

<!--
To make this work requires a couple a few things - first and foremost a test suite and test practices that give you confidence that issues won't make it to production - this strongly implies test first.

Second is that there are no long lived feature branches, possibly local tactical branches - but the aim is to actively avoid feature branches and by extension pull requests because those are slow and blocking.

Now that makes for an interesting dynamic in terms of culture and trust and there are far better people than me to explore that.
-->

---

# Challenges

* Don't break production
* No breaking changes

<!--
The core challenges kind of come down to

Don't break production
No breaking changes (for dependencies)

From a development point of view my challenge - as someone who is mostly a server side developer -  is that its a great many useful features can't be done in less than a day - so how do we develop - build and test - non-trivial features without ending up with a codebase we can't deploy.

Well the first thing is that we can write (with unit tests) a surprising amount of code without actually plumbing it in, for new functionality, for anything that additive (think of a new API endpoint) you can concievably get all the way to delivered without impacting anything else. But... sooner or later we're going to want to make changes that impact behaviour of live code paths and we're going to want to be able to test that - all still without breaking production.
-->
---

# Feature Flags

> Feature flags are a software development technique that allows teams to enable, disable or change the behavior of certain features or code paths in a product or service, without modifying the source code.

<!--
So Flags! 

Specifically feature flags I have borrowed a definition:

Feature flags are a software development technique that allows teams to enable, disable or change the behavior of certain features or code paths in a product or service, without modifying the source code.

Which is worth a bit of exploration and I may be able to show you something interesting

So what do we need to implement feature flags? There are all kinds of possibilites, but for me we can a long way with just "on" and "off" - and for rapid feature delivery that's substantially what we need.
-->

---

# Simplest possible thing that works

```json
{
    "FeatureManagement": {
        "ShinyNewFeature": true
    }
}
```

<!--
Clearly there are some limits - substantially we need to redeploy to twiddle the flag, but for a lot of deployments that's not a huge problem. And dotnet tooling lets us dynamically reload config - in theory at least!

And we can use that in code...
-->

---

# In code

```csharp
Console.WriteLine("Hello, World!");

if (configuration["FeatureManagement:ShinyNewFeature"] == "true")
{
    Console.WriteLine($"The time now is: {DateTime.Now}");
}
```

<!--
That works... but please don't do that... I've tidied that up a teeny bit - so lets just take a look at that which takes a couple of steps in the general direction of better
-->

---

# Demo 02

<!--
Don't use configuration directly, pretty much anywhere except startup
Define feature names in one place (makes it easier to remove the flag)
Naming matters - match to something meaningful
-->

---

# Thoughts...

1. It doesn't have to be complicated
1. It should still be good code

---

# Keeping code clean

- Demo 03

<!--
The first example is fairly trivial, but in the real world things are a bit more interesting

First we have to work out how to get the flag to the point we want to evaluate it.

There are a couple of simple choices - if we're just using simple toggles then it becomes config, if we're using a more complex evaluation - if things are dynamic - we may need to pass in a function or a class.

Even then we can end up with a situation were we have more complex code - we don't want to litter ifs all over the place and that may not even been practical - we need to isolate the old code and the new code and make the decision once.
-->

---

# Branch by Abstraction

* Demo 04

<!--
But if we're talking .NET and C# and assuming we have testable code we're likely to be relying on interfaces and that creates some nice possibilities - we can implement our new feature as a new implementation of the same interface. We can develop the class with tests but without actually plumbing it in (so safe to ship and deploy!) and we can then create a switching decorator that will call either one or the other depending on the flag. 
-->

---

# Nicer Branch by Abstraction

* Demo 05

<!--
Its the same interface so the consuming code doesn't care - this can get a bit messy to implement, but I'd imagine a code generator on the interface would be straightfoward.

One of the key benefits here is that there are no flags in the production code - the code we want to keep - its all abstracted into the wrapper. 

But that brings up another interesting question... what's the _scope_ of a feature flag - we generally don't want to change implementation midway through a request, so for transient classes we want to set the flag once per invocation.

I'm focusing on server side code, but in the client the same applies, generally you'd prefer the state not to change during a session...

That means that if we're using DI then we can have a factory method... and it gets really clean
-->

---

# Learning...

* Lifetime is important

---

# A small detour

Changing storage

<!--
We never do this right? Yeah...

I've seen this several times and I've seen it take way longer than it should and with all kinds of interesting complications

And I recently migrated our system from using mongo from files to using S3

It can be done transparently and without breaking changes - we just lean on interfaces again.
-->

---

# Step 1

* Read from old
* Write to both
* Validate new (read from both and compare)

<!--
We have an old repository and a new repository and a wrapper that implements the same interface.
To start and to validate our new storage we read from the old store and write to both. This lets us validate the new store
-->

---

# Step 2

* Read from new / both
    * Fall back to old
* Write to Both

<!-- 
Once we have some confidence we can start reading from the new store, with a fall back to the old store
We probably want to keep writing to both at this point
-->

---

# Step 3

* Read from new
    * Fall back to old
* Write to new
* Backfill new

<!--
Next step is to stop writing to the old store
We can back fill the new store with the old data
-->

---

# Step 4

* Read from new
* Write to new
* decommission old

<!--
Finally we can decommission the old store.

At this point we can look to take proper advantage of the new platform without - that may mean more migrations... but we know how to do that now!

The reason for the diversion is to emphasise that the flags in and of themselves are not enough, we need to thinks more deeply about how we are able to make changes relatively safely with an ability to roll back
-->

---

# Beyond Configuration

* Launch Darkly?

<!-- 
Ok, back on track, whilst its possible to a long way just with configuration (a surprisingly long way as it happens), there will come a point where you hit limits - you'll want to be able to change flags without deploying or used more advanced capabilities like targetting or progressive rollouts - and you may want to use flags for other purposes (one example I've seen whilst doing my homework is to schedule changes to banners say for Black Friday)

So does this mean its time for Launch Darkly? 

Or... since we're .NET folks...
-->

---

# Microsoft Feature Management

* Configuration Based
* Azure App Configuration
* SDKs for multiple languages

---

# Demo06

---

# How do I choose

* Capabilities
* Library support - does it work in the languages I use
* DX / UX
* Cost 
* Lock in

<!--
Can you do the kind of flags you want in a way that makes sense

Cost - allowing that running something yourself is never actually free

...well escaping the last is always _with difficulty_ but it turns out that the CNCF have an answer to that one
-->
---

![Open feature logo](openfeature.png)

<!-- 
What if there was a standard - in much the same way as for OpenTelemetry, one that dealt both with the consuming flags and ensuring that we could do all the interesting things we want to? That would be OpenFeature - which is incubating nicely and looks to have enough traction to be sticking around
-->

---

# Standardised API

![OpenFeature provider model](provider.png)

<!--
Its not a perfect answer - the SDKs are standardised _but_ you'll still need a language specific provider for your chosen back end. This includes Microsoft Feature Management, but not - so far as I can see - using Microsoft's flag schema (so not really yet)
-->

---

# Demo 08 (7 didn't work!)

* Flagd
* Flipt

<!--
Lets illustrate this will a couple of open source options
-->

---

# Flagd - https://flagd.dev/

> flagd is a feature flag evaluation engine. Think of it as a ready-made, open source, OpenFeature-compliant feature flag backend system.

<!-- 
This is interesting as there's no UI, which makes for a pretty reasonable developer experience, but for other users perhaps not so much.
-->
---

## Flag configuration

```json
{
  "$schema": "https://flagd.dev/schema/v0/flags.json",
  "flags": {
    "new-welcome-banner": {
      "state": "ENABLED",
      "variants": {
        "on": true,
        "off": false
      },
      "defaultVariant": "off",
      "targeting": { 
        "if": [
          { "ends_with": [{ "var": "email" }, "@example.com"] },
          "on",
          "off"
        ]
      }
    }
  }
}
```

---

# Flipt - https://github.com/flipt-io/flipt - https://flipt.io

> An enterprise-ready, GRPC powered, GitOps enabled, CloudNative, feature management solution

<!--
Flipt another open source offering, though with a commercial SaaS offering
-->

---

# UI!

![Example of Rollout (boolean) configuration in flipt](flipt-rollout.png)

<!-- 
It has a UI...
-->

---

# Demo 08 - code and things

<!--
A bit meta, we're going to change the source of the flags with a flag...

dotnet run . backend=flagd
-->

---

# Demo 11 (ooops...)

* Flags are not real time

<!--
Some of you may have seen some of the demos and noticed that we were infecting our code with the async virus.
It doesn't have to be that way - even with async methods most clients actually cache the settings for some amoutn of time - but it is possible to have a client without async, the flipt native client for example.

So lets do one more demo to illustrate something else important
-->
---

# Thoughts

* OpenFeature is not quite there yet

---

# HERE BE DRAGONS

## Clients and Servers

* A client that's a client needs a client SDK
* A client that's a server needs a server SDK

To talk to something

* That is a server that serves flags

They may well behave differently and will make different assumptions

<!--
Language is confusing - make sure you have the right versions of the right things for your development cases
-->

---

# HERE BE DRAGONS

## Microservices

* Don't attempt to coordinate across services
* Or attempt to coordinate flags between client (UI) and server
    * Can signal by other means
* DO use versioning of APIs / Messages / etc

---

# Combinatorial Explosion

![AI Generated image of exploding numbers](CombinatorialExplosion.png)

---

# Returning to our Pipeline

* The aim is to have code that doesn't break production
* You gate by testing with the production flags
* If the code passes _with production flags_ its good to go...
* QA etc is done with different flags

---

# Tell 'em what you told 'em

* It doesn't have to be complicated
* Start simple
* Naming matters
* Clean up as early as possible

---

# More...

* There are lots of options for services
    * Self hosted
    * SaaS
* Pay attention to your needs
    * Language SDKs / providers
    * functionaltiy
    * UX
* Local dev doesn't - shouldn't - have to lean on production service

--- 

# Still More...

* OpenFeature - capability is there
* OpenFeature - implementations catching up
* async / await 🤦

---

# Links

* Feature Toggles (aka Feature Flags)
  https://martinfowler.com/articles/feature-toggles.html
* Branch By Abstraction
  https://martinfowler.com/bliki/BranchByAbstraction.html
* OpenFeature
  https://openfeature.dev/
* Me
    * Bluesky - https://bsky.app/profile/murph.recumbent.co.uk
    * Github - https://github.com/recumbent


