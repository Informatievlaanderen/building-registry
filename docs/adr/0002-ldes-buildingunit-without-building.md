# 1. Record architecture decisions

Date: 2025-04-29

## Status

Accepted

## Context

LDES server can't handle reverse properties
```
{
  ...
  "gebouw": {
   "@reverse": "https://data.vlaanderen.be/ns/gebouw#bestaatUit",
   "@type": "@id",
   "@context": {
     "@base": "https://data.vlaanderen.be/id/gebouw/"
   }
  },
  ...
  "gebouw": 31547,
  ...
}
```

## Decision

We will remove the part described above from the context and content.

## Consequences

LDES will not be able to navigate from buildingunit to building.  
When fixed we'll need to reproduce all the messages.
