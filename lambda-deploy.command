#!/bin/sh

cd `dirname $0`

cd SearchConnpassLineBot
dotnet lambda deploy-function
