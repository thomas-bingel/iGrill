# iGrill
C# library to connect to the iGrill using UWP (Universal Windows Platform)


Work in Progress...




Using Configuration to persist selected device
Using Bluetooth LE
Using MQTT
Using UWP
Using Xaml WPF (Use MVVM)
Configuration (Settings)
Multilanguage?
Support iGrill 1,2,3
Starting App in CommandLine (Checkout, Build, Run)
Azure Pipeline




'''plantump
@startuml component
actor client
node app
database db

db -> app
app -> client
@enduml
'''


![uncached image](http://www.plantuml.com/plantuml/proxy?src=https://github.com/thomas-bingel/iGrill/raw/master/Weber/docs/diagrams/src/architecture_overview.puml)
