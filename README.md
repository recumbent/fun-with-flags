# fun-with-flags
Presentation on feature flags for .NET Newcastle 2024-11

## Running flagd

docker run --rm -it --name flagd -p 8014:8013 -v ${PWD}:/etc/flagd ghcr.io/open-feature/flagd:latest start --uri file:./etc/flagd/flags.json

## Running flipt

-d => -rm -it

docker run --rm -it  -p 8080:8080  -p 9000:9000  -v ${PWD}:/var/opt/flipt docker.flipt.io/flipt/flipt:latest

curl -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Cache-Control: no-store" -d '{"flagKey":"ShinyNewFeature","entityId":"8852dbc2-71ac-4dd9-b667-6d0d294d0a74","context":{},"namespaceKey":"default"}' http://localhost:8080/evaluate/v1/boolean

## Using Rider

https://blog.jetbrains.com/dotnet/2024/10/01/markdown-powered-slides-in-jetbrains-ides/

## Websites

https://github.com/flipt-io/flipt
https://www.flipt.io/
https://flagd.dev/
https://learn.microsoft.com/en-us/azure/azure-app-configuration/overview
https://openfeature.dev/
https://martinfowler.com/bliki/BranchByAbstraction.html
https://martinfowler.com/articles/feature-toggles.html