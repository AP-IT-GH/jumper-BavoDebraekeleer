# ML Agents Jumper

### Opdracht voor VR Experience
### door Bavo Debraekeleer (s120267)

---

## Inleiding

In deze opdracht wordt een Agent in Unity met Machine Learning geleerd om over een aankomend obstakel te springen.
Elke episode krijgt dit obstakel een willekeurige snelheid tussen de ingestelde minimum en maximum waarden.
Als uitbreiding vliegen er tegelijkertijd ook bonus obstakels over de Agent die deze kan pakken voor bonus punten.

![](pictures/inleiding.png ':size=100%')

---

## Materialen

Voor deze opdrachten is volgende software vereist:
- Unity 3D engine
- ML Agent Unity en python packages
- Anaconda Navigator
- PyTorch python module
- TensorBoard

---

## Methoden

Installeer de nodige software, maak een nieuw Unity project aan en voeg het ML Agents package toe aan het project.

Beloningen voor de ML Agent zijn als volgt:
 - Succes = AddReward(1.0f): obstakel voorbij gekomen zonder te raken.
 - Bonus = AddReward(0.5f): bonus punt gepakt. Plus succes geeft totaal van 1.5 .
 - Failure = SetReward(-1.0f): obstakel geraakt, dus niet over geraakt. Eindigt altijd met -1.0

### 1) Leer omgeving

![](pictures/leer_omgeving.png ':size=100%')

Stap 1 is een leeromgeving opzetten met de nodige elementen.
Deze elementen zijn:
	- een Agent
	- een baan (Road)
	- een obstakel (Obstacle)
	- een bonus obstakel (Bonus)

1. Om te beginnen gaan we voor elke type object een tag aanmaken. Voeg, in de Inspector window bij het selecteren van een object, volgende Tags toe:
	> - Agent
	> - Road
	> - Obstacle
	> - Bonus
	
	> ![](pictures/tags.png ':size=100%')

2. Voeg een cube toe en geef deze de naam en Tag **Agent**.
	> - Verander de Transform/Scale in 	`X: 0, Y: 0.5, Z: 0`.
	> - Voeg een Rigidbody component toe.
	> - Geef dit object een blauw materiaal.

3. Voeg een plane toe aan de scène en geef deze de naam en Tag **Road**.
	> - Verander de Transform/Position in `X: 0, Y: 0, Z: 0`.
	> - Verander de Transform/Scale in 	`X: 0.3, Y: 1, Z: 2`.
	> - Geef dit object een donker grijs materiaal.
	
4. Voeg een sphere toe en geef deze de naam en Tag **Obstacle**.
	> - Verander de Transform/Position in `X: 0, Y: 0.5, Z: 9`
	> - Verander de Transform/Rotation in `X: 0, Y: 180, Z: 0`.
	> - Verander de Transform/Scale in 	`X: 1, Y: 1, Z: 1`.
	> - Geef dit object een geel materiaal.
	
5. Duplicate het Obstacle en geef deze de naam en Tag **Bonus**.
	> - Verander de Transform/Position in `X: 0, Y: 5, Z: 9`.
	> - Geef dit object een groen materiaal.
	
6. Voeg een leeg object toe aan de scéné `Create empty` en noem het **Game Manager**.

![](pictures/objecten.png ':size=100%')
	

### 2) Scripting

Stap 2 is de juiste ML Agent scripts toevoegen aan de Agent als ook enkele script aanmaken en toevoegen.

1. Maak een script aan genaamd **CubeAgentRaysJumper**. Deze gaat de Agent aansturen en de nodige functies toevoegen voor het leerproces.
	> Voeg bovenaan de ML Agents dependencies toe.
	> ```c#
		using Unity.MLAgents;
		using Unity.MLAgents.Sensors;
		using Unity.MLAgents.Actuators;
	```
	
	> Vervolgens hebben we volgende variabelen nodig:
	> - De Rigidbody `body` van de Agent om het springen met zwaartekracht mogelijk te maken.
	> - Een `isGrounded` boolean om na te gaan wanneer de Agent op de baan staat.
	> - Een `isObstaclePassed` boolean om na te gaan wanneer een obstakel voorbij is gegaan zonder de Agent te raken.
	> - Een `jumpSpeed` om de snelheid en dus hoogte van het springen te kunnen instellen. Maak deze `[SerializeField]` om herinstelbaar te maken in de Unity editor.
	> ```c#
		private bool isGrounded = true;
		private Rigidbody body;
		private bool isObstaclePassed = false;
		[SerializeField] public float jumpSpeed = 10.0f;
	```
	
	> Bij het initialiseren kunnen we nu het Rigidbody component opvragen om deze eenvoudiger aan te spreken.
	> ```c#
		public override void Initialize()
		{
			base.Initialize();
			body = GetComponent<Rigidbody>();
		}
	```
	
	> We willen ook dat de Agent voor het Machine Learning zijn eigen positie kent. Zo weet deze wanneer en hoe hoog deze kan springen en een bonus punt pakt.
	> ```c#
		public override void CollectObservations(VectorSensor sensor)
		{
			sensor.AddObservation(this.transform.localPosition);
		}
	```
	
	> Nu gaan we de acties en de succes belonging bepalen.
	> Er is maar één actie namelijk springen `actionBuffers.ContinuousActions[0]`.
	> We willen echter enkel dat er gesprongen kan worden wanneer de AGent op de baan staat `if (isGrounded)`.
	> Het is belangrijk deze boolean vervolgens op false te zetten omdat de Agent nu aan het springen is.
	> Voor een succes wordt er gebruik gemaakt van een flag boolean die true kan gezet worden vanuit het Game Manager script via een public functie.
	> ```c#
		public void ObstacleHasPassed()
		{
			isObstaclePassed = true;
		}
	```
	> Wanneer deze true is wordt een beloning van 1 toegevoegd, de flag wordt gereset naar false, en de Episode wordt beëindigd.
	> We voegen hier ook logging toe met kleur om wat er gebeurd in het script visueel te maken tijdens het leren.
	> ```c#
		public override void OnActionReceived(ActionBuffers actionBuffers)
		{
			if (actionBuffers.ContinuousActions[0] == 1.0f)
			{
				if (isGrounded)
				{
					body.velocity = new Vector3(0, jumpSpeed, 0);
					Debug.Log("<color=blue>Jump!</color>");
					isGrounded = false;
				}
			}

			if (isObstaclePassed)
			{
				AddReward(1.0f);
				Debug.Log("<color=green>Succes!</color>");
				isObstaclePassed = false;
				EndEpisode();
			}
		}
	```
	
	> De volgende stap is collisions nagaan en de failure en bonus beloningen bepalen.
	> De eerste check zet de `isGrounded` op true wanneer de Agent terug geland is en op de baan staat.
	> Het tweede is de failure "beloning". Wanneer een Obstacle geraakt wordt is de totale beloning -1, ongeacht bonus punten. Het obstakel wordt gereset en de Episode beëindigd.
	> Wanneer een bonus punt gepakt wordt zal de beloning met 0.5 verhoogd worden. Dit is een `AddReward` dus dit is bovenop de eventuele succes beloning van 1.
	> Het Bonus object wordt hier eveneens, via het Obstacle script die ook hiervoor gebruikt wordt, gereset.
	> ```c#
		private void OnCollisionEnter(Collision collision)
		{
			Debug.Log($"Collision with {collision.gameObject.name}");

			if (collision.gameObject.CompareTag("Road"))
			{
				isGrounded = true;
			}
			
			if (collision.gameObject.CompareTag("Obstacle"))
			{
				SetReward(-1.0f);
				//AddReward(-1.0f);
				Debug.Log("<color=red>Failed!</color>");
				collision.gameObject.GetComponent<Obstacle>().Reset();
				EndEpisode();
			}

			if (collision.gameObject.CompareTag("Bonus"))
			{
				AddReward(0.5f);
				Debug.Log("<color=yellow>BONUS!</color>");
				collision.gameObject.GetComponent<Obstacle>().Reset();
			}
		}
	```
	
	> Als laatste is er nog de Heuristic methode die toelaat de leeromgeving handmatig te testen.
	> Als invoer voor het springen gebruiken we de standaard **Jump** button, op een toetsenbord de spatiebalk.
	> ```c#
		public override void Heuristic(in ActionBuffers actionsOut)
		{
			var continuousActionsOut = actionsOut.ContinuousActions;
			if (Input.GetButton("Jump"))
			{
				continuousActionsOut[0] = 1.0f;
			}
		}
	```

2. Voeg volgende scripts toe aan de Agent:
	> - **CubeAgentRaysJumper**
	> ![](pictures/script_CubeAgentRaysJumper.png ':size=100%')
	
	> - **Behavior Parameters**
	> ![](pictures/script_BehaviorParameters.png ':size=100%')
	
	> - **Decision Requester**
	> ![](pictures/script_DecisionRequester.png ':size=100%')
	
	> - 2x **Ray Perception Sensor 3D**
	> ![](pictures/script_RayPerceptionSensor3D.png ':size=100%')
	
	> Het **Behavior Parameters** script heeft vervolgens behavior parameters nodig in de vorm van een .yaml file.
	> Maak in de project folder `./MLAgents/config/CubeAgent.yaml` met volgende parameters:
	```yaml
		behaviors:
		
			CubeAgentRay:

			trainer_type: ppo

			hyperparameters:

			  batch_size: 10

			  buffer_size: 100

			  learning_rate: 0.0003

			  beta: 0.005

			  epsilon: 0.2

			  lambd: 0.99

			  num_epoch: 3

			  learning_rate_schedule: linear
			  
			  beta_schedule: constant
			  
			  epsilon_schedule: linear

			network_settings:

			  normalize: false

			  hidden_units: 128

			  num_layers: 2

			  vis_encode_type: simple

			reward_signals:

			  extrinsic:

				gamma: 0.90

				strength: 1.0
				
			  curiosity:
				
				strength: 5.0
				
				gamma: 0.99
				
				encoding_size: 256
				
				learning_rate: 1e-3

			keep_checkpoints: 5

			max_steps: 2500000

			time_horizon: 64

			summary_freq: 2000

			threaded: true
	```
	> Vul vervolgens in het script component **Behavior Parameters** onder **Behavior Name** de naam van de aangemaakte behavior:
	> `Behavior Name: CubeAgentRay`
	
	> De eerste **Ray Perception Sensor 3D** wordt als volgt ingesteld:
	> `Sensor Name: RayPerceptionSensor_Straight`
	> `Rays Per Direction: 1`
	> `Max Ray Degrees: 10`
	> `Sphere Cast Radius: 0.55`
	> `Ray Length: 7`
	> `Stacked Raycasts: 1`
	> `Start Vertical Offset: 0`
	> `End Vertical Offset: 0`
	
	> De tweede **Ray Perception Sensor 3D** wordt als volgt ingesteld:
	> `Sensor Name: RayPerceptionSensor_Straight`
	> `Rays Per Direction: 1`
	> `Max Ray Degrees: 10`
	> `Sphere Cast Radius: 0.55`
	> `Ray Length: 7`
	> `Stacked Raycasts: 1`
	> `Start Vertical Offset: 0`
	> `End Vertical Offset: 5`

3. Maak een script aan genaamd **Obstacle**. Deze gaat zowel het Obstacle als Bonus object aansturen.
	> We hebben volgende variabelen nodig:
	> - `obstacle Speed` om de snelheid van het object aan te geven.
	> - Instelbare minimum en maximum waarden, `minSpeed` en `maxSpeed`, voor de willekeurige snelheidstoekenning.
	> - Een extra variabele `obstacleSpeedMultiplier` om de snelheid vervolgens in het algemeen te kunnen vertragen of versnellen
	> - En een variabele `startPos` die de oorspronkelijke positie van het obstakel bijhoud om naar te kunnen resetten.
	> ```c#
		private float obstacleSpeed = 0.0f;
		[SerializeField] private int minSpeed = 2;
		[SerializeField] private int maxSpeed = 5;
		[SerializeField] private float obstacleSpeedMultiplier = 1.0f;
		private Vector3 startPos;
	```
	
	> Bij het starten wordt eerst de start positie van het object opgeslagen.
	> Vervolgens wordt een willekeurige snelheid bepaald met volgende functie:
	> ```c#
		private void SetRandomSpeed()
		{
			obstacleSpeed = Random.Range(minSpeed, maxSpeed + 1) * obstacleSpeedMultiplier;
			Debug.Log($"obstacleSpeed={obstacleSpeed}");
		}
	```
	> De start ziet er als volgt uit:
	> ```c#
		void Start()
		{
			startPos = transform.position;
			SetRandomSpeed();
		}
	```
	
	> Vervolgens gaan we in de Update methode het object voorwaarts laten bewegen aan de hand van de willekeurige snelheid.
	> ```c#
		void Update()
		{
			transform.position += obstacleSpeed * Time.deltaTime * transform.forward;
		}
	```
	
	> Als laatste is er de public Reset methode om vanuit andere script te kunnen aanroepen.
	> Hiermee wordt het object naar zijn start positie teruggezet en een nieuwe willekeurige snelheid bepaald.
	> ```c#
		public void Reset()
		{
			transform.position = startPos;
			SetRandomSpeed();
		}
	```
	
4. Voeg nu het Obstacle script toe als component op zowel het Obstacle als het Bonus object.
	> ![](pictures/script_Obstacle.png ':size=100%')

5. Als laatste script, maak het script **GameManager** aan.
	> Voor de Game Manager hebben we volgende variabelen nodig:
	> - De objecten `Agent` `Obstacle` en `Bonus` om te kunnen aanspreken.
	> - De start posities `obstacleStartPos` en `bonusStartPos` van Obstacle en Bonus.
	> - [Optioneel] Een bool om de bonus punten in en uit te kunnen schakelen.
	> ```c#
		private GameObject Agent;
		private GameObject Obstacle;

		private Vector3 obstacleStartPos;

		[SerializeField] private bool isWithBonus = false;
		private GameObject Bonus;
		private Vector3 bonusStartPos;
	```
	
	> De start methode zoekt de objecten aan de hand van de Tags.
	> [Optioneel] Het Bonus object wordt enkel gezocht als dit is ingeschakeld. Dit om errors te voorkomen als deze niet aanwezig is in de scène.
	> ```c#
		void Start()
		{
			Agent = GameObject.FindGameObjectWithTag("Agent");
			Obstacle = GameObject.FindGameObjectWithTag("Obstacle");

			obstacleStartPos = Obstacle.transform.localPosition;

			if (isWithBonus)
			{
				Bonus = GameObject.FindGameObjectWithTag("Bonus");
				bonusStartPos = Bonus.transform.localPosition;
			}
		}
	```
	
	> In de update methode word nagegaan of de Obstacle en Bonus objecten aan het einde van de baan gekomen zijn en dus voorbij de Agent zijn gegaan.
	> In het geval van het Obstacle wordt dit doorgegeven aan het CubeAgentRaysJumper script om de succes succes flag te zetten.
	> Verder wordt in beide gevallen de reset methode van de objecten aangeroepen om deze te resetten.
	> [Optioneel] De checks op het Bonus object worden uiteraard enkel uitgevoerd als dit is ingeschakeld. Dit om errors te voorkomen.
	> ```c#
		void Update()
		{
			if(Obstacle.transform.localPosition.z < obstacleStartPos.z - 18.0f)
			{
				Agent.GetComponent<CubeAgentRaysJumper>().ObstacleHasPassed();
				Obstacle.GetComponent<Obstacle>().Reset();
				Debug.Log("Obstacle passed.");
			}

			if (isWithBonus)
			{
				if (Bonus.transform.localPosition.z < bonusStartPos.z - 18.0f)
				{
					Bonus.GetComponent<Obstacle>().Reset();
					Debug.Log("Bonus passed.");
				}
			}
		}
	```
	
6. Voeg het **GameManager** script toe aan het **Game Manager** object.
	> ![](pictures/script_GameManager.png ':size=100%')

7. [Optioneel] Maak vervolgens een Prefab van de leeromgeving.
	> Maak een leeg object aan `Create empty` en noem deze **TrainingAreaRays_Jumper**.
	> Sleep de objecten **Agent**, **Road**, **Obstacle** en **Bonus** in dit object.
	> Maak een Prefab map aan in het project en sleep het object hierin om de Prefab te creëren.
	
Nu is de leeromgeving in Unity compleet.

![](pictures/leer_omgeving.png ':size=100%')


### 3) Machine Learning

![](pictures/inleiding.png ':size=100%')

1. Start **Anaconda Navigator**.
2. Launch een **CMD.exe Prompt** vanuit de navigator.
3. Navigeer naar de project folder aan de hand van `cd`.
4. Om het trainen te starten geef volgend commando: `mlagents-learn config/CubeAgent.yaml --run-id=CubeAgentJumper`.
	> Als het trainen gestopt is kan je verder gaan door ` --resume` toe te voegen aan dit commando.
	> Om het trainen onder dezelfde run-id te herstarten voeg ` --force` toe.
5. Wacht even tot dit is opgestart en druk op **Play** in de Unity editor wanneer dit gezegd wordt in de prompt.
	> ![](pictures/training.png ':size=100%')

Vervolgens kan je het leerproces volgen met de TensorBoard web app.
1. Launch nog een **CMD.exe Prompt** vanuit de navigator.
2. Navigeer naar de project folder aan de hand van `cd`.
3. Voer volgend commando in: `tensorboard --logdir results`.
4. Open een browser en ga naar `http://localhost:6006/`
	> ![](pictures/tensorboard_prompt.png ':size=100%')
	
	> ![](pictures/tensorboard.png ':size=100%')

Na het succesvol trainen en het produceren van een .onnx file kan je deze toevoegen Agent.
1. Navigeer naar de project map.
2. Kopieer het bestand `./results/CubeAgentJumper/CubeAgentRay.onnx` naar `./Assets/Agent Models/`.
3. In de Unity editor sleep dit bestand in het **Agent** object, in het **Behavior Parameters** component, in het **Model** veld.
4. Wanneer nu op **Play** wordt gedrukt zal dit model automatisch spelen. 
	> ![](pictures/script_BehaviorParameters.png ':size=100%')

---

## Resultaat

### Leerproces resultaten

Volgende diagrammen zijn een voorbeeld van het leerproces.
Hier werd na 600k episodes succesvol 1.5 beloning gehaald, waarna de training stilviel door een error (vermoedelijk door geheugen gebrek) waardoor de Agent niet meer sprong.
Bij heropstart kwam deze wel direct weer gemiddeld aan 1.0 beloning.

#### Cumulative_Reward
![](pictures/Environment_Cumulative_Reward.svg ':size=100%')

#### Episode Length
![](pictures/Environment_Episode_Length.svg ':size=100%')


### Panopto video

Bekijk <a href="https://ap.cloud.panopto.eu/Panopto/Pages/Viewer.aspx?id=b1cd7aa3-05a5-4594-b799-ae98009c441e">hier</a> een opname van het resultaat in actie.