# AI Product Requirements Document
Add two API Methods - Firebase Topics

## Problem
Add two API methods for subscribeTokensToTopic and unsubscribeTokensFromTopic that take arrays of tokens and call the firebase APIs

## Goals 
Firebase (external API) provides two API functions for topic subscription management:

•	SubscribeToTopicAsync
•	UnsubscribeFromTopicAsync

When sending a message to firebase, the message object has an instruction to either be sent to a specific token, or a specific topic, or to multiple topics. The functions above take arrays of tokens for their operations.

Currently, Redi-Mart commerce server has three firebase functions:

•	SubscribeTokenToTopic
•	SendPushNotificationToToken
•	SendPushNotificationToTopic

The subscribe function only subscribes a single token. 
(You can find it in file service/interface/iucjson.cs and service/interface/iucservice.cs. 
It's implementation is in service/common.LSOmniBaseCustom.cs)


## Non‑Goals
What it shouldn't do e.g. AI‑driven recommendation, ranking, or optimization

## Users & Context
End user: Business central user
System context: business central will call our api to interact with firebase
Trigger condition: user in business central is managing firebase topics

## Inputs and assumptions
Ask me rather than assuming things.
## Outputs and success criteria
two new API functions and their implemntations.
## Behavioral Rules
Plan for the changes and get confirmation first.
## Quality Metrics
## Risks & Mitigations
## Out of Scope