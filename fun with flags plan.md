# Fun with Flags

There is strong evidence to suggest that the best way to work is trunk driven development - enough smart people being successful and with reasonable evidence - DORA metrics - to suggest its certainly something we should look at.

---

To make this work we have to do proper continuous integration - pushing our code to the trunk (not to a branch, but to the main branch we build and deploy from) at least once a day.

Inherent in this is the assumption that if you push code it _will_ go into production (that leads to continuous delivery and contninuous deployment)

---

To make this work requires a couple a few things - first and foremost a test suite and test practices that give you confidence that issues won't make it to production - this strongly implies test first.

Second is that there are no long lived feature branches, possibly local tactical branches - but the aim is to actively avoid feature branches and by extension pull requests because those are slow and blocking.

Now that makes for an interesting dynamic in terms of culture and trust and there are far better people than me to explore that.

The challenge though is that its a great many useful features can't be done in less than a day - so how do we develop - build and test - non-trivial features without ending up with a codebase we can't deploy.

Well the first thing is that we can write (with unit tests) a surprising amount of code without actually plumbing it in, for new functionality, for anything that additive (think of a new API endpoint) you can concievably get all the way to delivered without impacting anything else. But... sooner or later we're going to want to make changes that impact behaviour of live code paths and we're going to want to be able to test that - all still without breaking production.

So Flags! 

Specifically feature flags I have borrowed a definition:

Feature flags are a software development technique that allows teams to enable, disable or change the behavior of certain features or code paths in a product or service, without modifying the source code.

Which is worth a bit of exploration and I may be able to show you something interesting

So what do we need to implement feature flags? There are all kinds of possibilites, but for me we can a long way with just "on" and "off" - and for rapid feature delivery that's substantially what we need.

---

That means that the easiest implementation is just a config value...

Sample code.

Clearly there are some limits - substantially we need to redeploy to twiddle the flag, but for a lot of deployments that's not a huge problem. And dotnet tooling lets us dynamically reload config using the options pattern.

---

There's a couple of more things I would probably add


Slightly more sample code.

This is gives us two learning points

1. You don't _need_ 3rd party tooling (at least not for server side code) to be effective. You can make something similar work in client side code too

The first example is fairly trivial, but in the real world things are a bit more interesting

First we have to work out how to get the flag to the point we want to evaluate it.

There are a couple of simple choices - if we're just using simple toggles then it becomes config, if we're using a more complex evaluation - if things are dynamic - we may need to pass in a function or a class.

Samples

Even then we end up with a situation were we have more complex code - we don't want to litter ifs all over the place and that may not even been practical - we need to isolate the old code and the new code and make the decision once.

We can start with methods

Sample

But if we're talking .NET and C# and assuming we have testable code we're likely to be relying on interfaces and that creates some nice possibilities - we can implement our new feature as a new implementation of the same interface. We can develop the class with tests but without actually plumbing it in (so safe to ship and deploy!) and we can then create a switching decorator that will call either one or the other depending on the flag. 

Sample

Its the same interface so the consuming code doesn't care - this can get a bit messy to implement, but I'd imagine a code generator on the interface would be straightfoward.

One of the key benefits here is that there are no flags in the production code - the code we want to keep - its all abstracted into the wrapper. 

But that brings up another interesting question... what's the _scope_ of a feature flag - we don't want to change implementation midway through a request, so for transient classes we want to set the flag once per invocation.

I'm focusing on server side code, but in the client the same applies, generally you'd prefer the state not to change during the session... 

If we're using DI then we can have a factory method

Sample code

:think: actually that can work for singleton's too...

Realistically there are limits to what you can do with just configuration - so to address that there is Launch Darkly... and a quite astonishing number of alternatives.

As we're fans of .NET and azure, lets take a look at bits of _Microsoft's_ solution

Which, as it turns out is config based.

A bit more code...

Schema for feature flags

their party trick here is to just use another config provider. What's elegant about this is that we can use local config to set up an environment for testing but use the centralised stuff in deployed environments - and it _just works_ because the flag mechanism doesn't care.

Now an azure based solution won't work for everyone and there are other issues for more advanced use of flags

So the question then becomes _how do I choose_ - there are a few things we care about

Capabiities
Library support - does it work in the languages I use
DX
Cost 
Lock in

Quick note on library / SDK support
There are two types of library Client and Server where, confusingly server libraries are for clients of the service that are servers... the assumptions about behaviour need to be different

Well escaping the latter is always _with difficulty_ but it turns out that the CNCF have an answer to that one

OpenFeature

Some examples with their SDK

Except...

Example with flipt

Other thoughts

Timing, no shared flags

Clean up behind you - aggressively


---

Plan:
* Intro what problem are we trying to solve
* Simplest thing that can work
    * Direct reference to config with a string
    * Tidy up a bit
* Commentary on types of flags
* Patterns
    * Two versions of a method (C# inline)
    * Branch by abstraction - decorator
    * Branch by abstraction - factory
* Commentary on lifecycle
* Diversion - migrations
    * No breaking changes
    * Combination of code and flags
* Tooling - what happens when you want to do more?
    * Launch Darkly... 
    * There are other choices
* Microsoft library - builds on config, expand existing example
* Nice developer experience _because its just config_
* Thoughts - client vs server
* Example of _client_ code - either here or after open feature
* Something about cacheing - look at the MS docs
* What if you're not using azure or have other constraints, how do you pick a vendor, how do you avoid issue with lock in, wouldn't it be nice if there was a standard...
* Openfeature.dev
* Two examples - flipt + something else, change flags by changing provider
* Thoughts
    * Naming of things (somewhere)
    * lifecycle - tidy up aggressively
* Thoughts
    * Back to where we started - what flags should we be testing with
    * Config and dependencies are a different, but related, problem
* Has anyone spotted the problem with the evaluation code
    * Async - virus
* Example using flipt's own libraries
* Closing thoughts
    * You have to be considered

Missing:
- Don't assume that changes are instant
- Don't share flags - because not instant
    - Client / Server (potentially send flag as a header)
    - Across services
- Do make sure you're logging
- Where appropriate (APIs, Messages) use versioning - behind a switch :)
