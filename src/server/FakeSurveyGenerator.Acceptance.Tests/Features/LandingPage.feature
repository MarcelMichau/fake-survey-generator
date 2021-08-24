@LandingPage
Feature: Landing Page
Initial page shown when navigating to the application

Scenario: Application Name & API Version is Displayed
	When navigating to the application URL
	Then the name of the application is displayed
	And the version number of the API is Displayed