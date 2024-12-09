using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Tarea3_SyP
{
    
    /// <summary>
    /// Pequeño juego de escape, en la que ayudado por items aleatorios que pueden ser activados, 
    /// un astronauta intenta llegar a su nave antes de que los alienigenas malos lo cogan.
    /// Posee dos modos de juego, que se seleccionan en el menu. En el primero, aparecerán items para activar
    /// en grupos de 4 de forma aleatoria sobre el dispositivo de control. En el segundo, un solo tipo de item aparece en posiciones aleatorias
    /// dentro del dispositivo de control.
    /// Los items positivos requieren de doble click para ser activados. Los negativos solo necesitan un solo click.
    /// En este codigo estan incluidas partes comentadas, que son codigo no implementado, para que se pueda ver lo que hice
    /// y lo que no consegui hacer tambien. 
    /// *****NOTA*****
    /// Cuando hago refercnia a "botones", la mayoria del tiempo me estoy refiriendo a elementos de tipo "pictureBox", Utilicé este tipo de elemento
    /// de la interfaz para poder jugar con las trasnparencias para todos los elementos de la interfaz
    /// </summary>
    public partial class Form1 : Form
    {
        //atributos de clase que seran accesibles desde varias partes del programa
        private bool _funcionando;//boleano para comprobar el estado de la aplicacion
        private bool _pausaAstronauta; //detiene el movimiento del astronauta
        private bool _pausaMalutos; //detiene el avance de los malos
        private bool _pararHilos; //detiene todos los hilos activos
        private Thread _malutoThread, _astronautaThread, _pulsadoresThread, _sonidoBaseThread;

        //inicializo los distintos bancos de sonido para que no haya que hacerlo en mitad de la ejecucion del programa y facilitar los cambios
        //de sonidos en caso de querer modificarlos
        System.Media.SoundPlayer musicaBase = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\musicaBase.wav");
        System.Media.SoundPlayer bug = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\bug.wav");
        System.Media.SoundPlayer cosaBuena = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\cosaBuena.wav");
        System.Media.SoundPlayer paused = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\pause.wav");
        System.Media.SoundPlayer x1back = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\backwards.wav");
        System.Media.SoundPlayer muerte = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\muerte.wav");
        System.Media.SoundPlayer menu = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\pulsarMenu.wav");
        System.Media.SoundPlayer cogido = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\cogido.wav");
        System.Media.SoundPlayer victoria = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\victoria.wav");
        System.Media.SoundPlayer start = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\start.wav");

        private delegate void Delegado();

        public Form1()
        {
            
            InitializeComponent();
            _funcionando = true; //aseguramos que el programa este funcionando al cargar el formulario
            _pararHilos = false; //nos aseguramos que lo hilos no estan detenidos
        }

        //instrucciones qeu se ejecutan al cargar el formulario
        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            //inicio de un hilo para cargar musica inicial
            _sonidoBaseThread = new Thread(MetodoDelegado4);
            //_sonidoBaseThread.IsBackground = true;
            _sonidoBaseThread.Start();
        }

        #region MetodosDelegados

        /// <summary>
        /// Metodo delegado en el que se llama al movimiento de los malos, asi como se define las 
        /// caracteristicas de su parada
        /// </summary>
        private void MetodoDelegado()
        {
            while (_funcionando && !_pararHilos) 
            {
                if (_pausaMalutos) 
                {
                    //en caso que paren los malos, invocamos una accion con el metodo Invoke, para acceder de forma segura al hilo de la interfaz
                    Invoke(new Action(() =>
                    {
                        //cambimos las propiedades de los picturebox de los malos para que muestren la imagen correcta en un tamaño equiparable
                        //al que tienen cuando muestran la imagen de moviiento
                        malutoRojo.Image = Properties.Resources.maloparado_rojo_ezgif_com_effects;
                        malutoRojo.Size = new Size(80, 60);
                        malutoAzul.Image = Properties.Resources.maloparado_azul_ezgif_com_effects;
                        malutoAzul.Size = new Size(80, 60);
                        malutoNaranja.Image = Properties.Resources.maloparado_ezgif_com_gif_maker;
                        malutoNaranja.Size = new Size(80, 60);
                        malutoVerde.Image = Properties.Resources.maloparado_verde_ezgif_com_effects;
                        malutoVerde.Size = new Size(80, 60);
                    }));

                    Thread.Sleep(3000);//mantenemos la parada por 3 segundos

                    Invoke(new Action(() =>
                    { 
                        //invocamos la accion que restaura el aspecto normal de los picturebox de los malutos
                        malutoRojo.Image = Properties.Resources.maloamdando_rojo_ezgif_com_effects;
                        malutoRojo.Size = new Size(61, 38);
                        malutoAzul.Image = Properties.Resources.maloamdando_azul_ezgif_com_effects;
                        malutoAzul.Size = new Size(61, 38);
                        malutoNaranja.Image = Properties.Resources.maloamdando_ezgif_com_gif_maker;
                        malutoNaranja.Size = new Size(61, 38);
                        malutoVerde.Image = Properties.Resources.maloamdando_verde_ezgif_com_effects;
                        malutoVerde.Size = new Size(61, 38);
                    }));

                    _pausaMalutos = false;//deteneomos la pausa de los Malutos
                }
                else
                {
                    Movimiento(); //llamamos al metodo que controla el movimiento de los malutos
                    Thread.Sleep(50);
                }

            }
        }

        /// <summary>
        /// Metodo delegado en el que se llama al movimiento del astronauta, asi como se define las 
        /// caracteristicas de su parada
        /// </summary>
        private void MetodoDelegado2()
        {
            while (_funcionando && !_pararHilos)
            {
                if (_pausaAstronauta)

                {
                    //en caso que el astronauta este parado, cambiamos la imagen del picture box por 2 segundos para reflejar este estado de forma visual.
                    //a continuacion restauramos la imagen de movimiento standard
                    Invoke(new Action(() =>
                    {
                        astronauta.Image = Properties.Resources.astronautasparado;
                    }));

                    Thread.Sleep(2000);

                    Invoke(new Action(() =>
                    {
                        astronauta.Image = Properties.Resources.tiron_graphics_astronaut_animated;
                    }));

                    _pausaAstronauta = false;
                }
                else
                {
                    MovimientoAstronauta();//Si no esta parado, llamamos al metodo que define el patron de movimiento del astronauta
                    Thread.Sleep(200);
                }

            }
        }

        /// <summary>
        /// Metodo delegado en el que recogemos la posicion del radio button delc formulario y
        /// llamamos a los metodos correspondientes para cada caso
        /// </summary>
        private void MetodoDelegado3()
        {
            while (_funcionando && !_pararHilos)
            {
                if (multiControl.Checked)
                {
                    RandomizadorBotones();
                }
                if (botonLoco.Checked)
                {
                    RandomizadorPosicion();
                }
            }
        }

        /// <summary>
        /// metodo delegado para llamar al metodo que reproduce la musica de fondo
        /// </summary>
        private void MetodoDelegado4()
        {
            PlayMusicaBase();
        }

        #endregion

        //metodos que controlan los patrones de movimiento y la mayoria de condiciones de victoria
        #region RutinasMovimiento

        /// <summary>
        /// metodo que determina el patron de movimiento de los Malutos, asi como su condicion de victoria
        /// </summary>
        private void Movimiento()
        {

            Random aleatorio = new Random();

            //generamos patron de moviento aleatorio que desplazara a la derecha sus picturebox de 0 a 9 pixeles cada iteracion
            malutoRojo.Left += aleatorio.Next(10);
            malutoAzul.Left += aleatorio.Next(10);
            malutoVerde.Left += aleatorio.Next(10);
            malutoNaranja.Left += aleatorio.Next(10);

            //Condicion de victoria para cada maluto, en la que si su posicion es igual a la de la mitad del cuerop del astronauta se considera que lo han atrapado
            //Enese caso, se paran todos los hilos, suena el audio correspondiente, se muestra un mensaje con el resultado y se reinicia la aplicacion
            if (malutoRojo.Left >= astronauta.Left - (astronauta.Width / 2))
            {
                _pararHilos = true;
                //System.Media.SoundPlayer cogido = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\cogido.wav");
                cogido.PlaySync();

                MessageBox.Show("Maluto Rojo te ha matado");
                Application.Exit();
                Application.Restart();
            }
            else if (malutoAzul.Left >= astronauta.Left - (astronauta.Width / 2))
            {
                _pararHilos = true;
                //System.Media.SoundPlayer cogido = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\cogido.wav");
                cogido.PlaySync();

                MessageBox.Show("Maluto Azul te ha matado");
                Application.Exit();
                Application.Restart();

            }
            else if (malutoVerde.Left >= astronauta.Left - (astronauta.Width / 2))
            {
                _pararHilos = true;
                //System.Media.SoundPlayer cogido = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\cogido.wav");
                cogido.PlaySync();

                MessageBox.Show("Maluto Verde te ha matado");
                Application.Exit();
                Application.Restart();
            }
            else if (malutoNaranja.Left >= astronauta.Left - (astronauta.Width / 2))
            {
                _pararHilos = true;
                //System.Media.SoundPlayer cogido = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\cogido.wav");
                cogido.PlaySync();

                MessageBox.Show("Maluto Naranja te ha matado");
                Application.Exit();
                Application.Restart();
            }
        }

        /// <summary>
        /// Metodo que determina el patron de movimiento del astronauta, asi como su condicion de victoria
        /// </summary>
        private void MovimientoAstronauta()
        {
            //decidi prescindir de movimiento aleatorio para el astronauta,dando uyn rango de movimiento minimo, para obligar a tirar de destreza para ganar el juego
            //Random aleatorio2 = new Random();
            astronauta.Left += 1;
            Thread.Sleep(20);

            //Situo la posicion de llegada a la nave como una proporcion del ancho total de la imagen, en caso de que se escale, se podria mantener el punto relativo de llegada
            //En la misma posicion visual, la puerta de la nave. Si se alcanza ese punto, se detiene los hilos, se reproduce el audio de victoria, se da un mensaje y se sale y reinicia la aplicacion.
            if (astronauta.Left >= this.Width * 0.88 - astronauta.Width)
            {
                _pararHilos = true;
                //System.Media.SoundPlayer victoria = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\victoria.wav");
                victoria.PlaySync();

                MessageBox.Show("A la nave");

                Application.Exit();
                Application.Restart();

            }
        }

        /// <summary>
        /// metodo para definir la aparicioon de botones en el modo de juego multiboton
        /// </summary>
        private void RandomizadorBotones()
        {
            while (_pararHilos == false && _funcionando) //mientras los hilos no esten parados y el programa funcionando
            {
                //se genera con los distintos items disponibles para cada posicion del cuadro de control del traje. Los items estan en posiciones descolocadas aposta
                //para generar una secuencia variada con el proceso de un solo numero aleatorio
                PictureBox[] botonesA = new PictureBox[] { a4x, a1back, a2x, a1x, abug, apause, amuerte };
                PictureBox[] botonesB = new PictureBox[] { b1x, bmuerte, b2x, b1back, bbug, b4x, bpause };
                PictureBox[] botonesC = new PictureBox[] { cmuerte, c1back, cpause, c4x, cbug, c2x, c1x };
                PictureBox[] botonesD = new PictureBox[] { dpause, dbug, d1back, dmuerte, d1x, d4x, d2x };

                Random random = new Random();
                Random tiempo = new Random();
                int botonVisible = random.Next(botonesA.Length);//se genera un umero aleatorio entre los valores indice de los arrys creados

                //Se usa el metodo Invoke, para acceder de forma segura al hilo del interfaz, y mostrar los items de cada array que se encuentran en el indice aleatorio creado
                //estos items se mostraran un tiempo aleatorio que irá desde 0.3 segundos a 2 segundos. A continuacion, se ocultan de nuevo y se reinicia el ciclo,
                //mientras los hilos no esten detenidos y el programa funcionando
                Invoke(new Action(() =>
                {
                    //******Intenté hacer que hubiera un sonido al pintar los iconos en la pantalla, pero debido a la velocidad de refresco, los sonidos se pisaban demasiado
                    //System.Media.SoundPlayer mostrarIconos = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\menus.wav");
                    //mostrarIconos.Play();
                    
                    botonesA[botonVisible].Visible = true;
                    botonesB[botonVisible].Visible = true;
                    botonesC[botonVisible].Visible = true;
                    botonesD[botonVisible].Visible = true;
                }));
                Thread.Sleep(tiempo.Next(300, 2000));


                Invoke(new Action(() =>
                {
                    botonesA[botonVisible].Visible = false;
                    botonesB[botonVisible].Visible = false;
                    botonesC[botonVisible].Visible = false;
                    botonesD[botonVisible].Visible = false;
                }));
                Thread.Sleep(50);
            }
        }


        /// <summary>
        /// metodo para definir la posicion donde va a aparecer el item del juego ocn un solo boton
        /// </summary>
        private void RandomizadorPosicion()
        {

            Random randX = new Random();
            Random randY = new Random();
            Random tiempo = new Random();

            Invoke(new Action(() =>
            {
                //System.Media.SoundPlayer mostrarIconos = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\menus.wav");
                //mostrarIconos.Play();

                //definimos de forma aleatria la posicion donde se mostrará el item, eligiendo como maximos y minimos la posicion de cuadro de control del traje del
                //astronauta, y mostramos el item por un timepo aleatorio ente 0.3 y 1.1 segundos. A continuacion, el ciclo se refresca mientras los hilos sigan
                //activos y el programa funcionando

                d4x.Left = randX.Next(137, 821);
                d4x.Top = randY.Next(625, 705);
                d4x.Visible = true;
            }));

            Thread.Sleep(tiempo.Next(300, 1100));

            Invoke(new Action(() =>
            {
                d4x.Visible = false;
            }));
            Thread.Sleep(300);


        }

        #endregion

        //eventos provocados por la pulsacion de los botones de control del menu de la interfaz
        #region BotonesMenu

        
        private void Apagar_Click(object sender, EventArgs e)
        {
            //sonido de click de menu y cierre de la aplicacion
            
            menu.PlaySync();

            Application.Exit();
        }

        private void Reiniciar_Click(object sender, EventArgs e)
        {
            //sonido de click de menu y reinicio de la aplicacion
            menu.PlaySync();

            Application.Restart();
        }

        private void Tutorial_Click(object sender, EventArgs e)
        {
            //reproduccion de forma sincrona(para que pueda terminar un sonido y empieze el otro) del clcik de menu y de la musica de
            //fondo del programa, para seguirla escuchando mientras se lee el tutorial
            //System.Media.SoundPlayer menu = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\pulsarMenu.wav");
            menu.PlaySync();
            musicaBase.Play();

            //messagebox que lee el contenido de un archivo txt alojado en la raiz de la aplicacion
            MessageBox.Show(File.ReadAllText(Application.StartupPath + "tutorial.txt"));
        }
       
        private void Jugar_Click(object sender, EventArgs e)
        {
            //reproduccion del audio click de menu y llamada al metodo que inicia el juego
            //System.Media.SoundPlayer start = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\start.wav");
            start.PlaySync();
            StartGame();
        }
        #endregion

        //eventos porvocados por la pulsacion de botones de juego
        #region Pulsadores

        //pulsadores de juego activos
        #region En uso

        //eventos que reproducen el sonido correspondiente a cada item y llaman a la funcion que ejecuta la accion correspondiente
        //Los items positivos responden a eventos de tipo "dobleClick", mientras qeu los negativos a eventos "click" .No quise ser tan mal persona de activarlos 
        //con el evento mousehover ;). Se implementan los eventos para los botones del grupo "a", de los 4 grupos de botones existentes. Los botones de los 
        //grupos "b", "c", y "d" llaman al evento de sus correspondientes botones del grupo "a" para economizar codigo
        private void a4x_DoubleClick(object sender, EventArgs e)
        {
            //System.Media.SoundPlayer x4 = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\cosaBuena.wav");
            cosaBuena.Play();
            MoveFordwardEnergize();
        }

        private void abug_DoubleClick(object sender, EventArgs e)
        {
            //System.Media.SoundPlayer bug = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\bug.wav");
            bug.Play();

            AlienBug();
        }

        private void a2x_DoubleClick(object sender, EventArgs e)
        {
            //System.Media.SoundPlayer x2 = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\cosaBuena.wav");
            cosaBuena.Play();
            MoveFordward2x();
        }

        private void a1x_DoubleClick(object sender, EventArgs e)
        {
            //System.Media.SoundPlayer x1 = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\cosaBuena.wav");
            cosaBuena.Play();
            MoveFordward1x();
        }

        private void apause_Click(object sender, EventArgs e)
        {
            //System.Media.SoundPlayer pause = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\pause.wav");
            paused.Play();
            PauseAstronauta();
        }

        private void a1back_Click(object sender, EventArgs e)
        {
            //System.Media.SoundPlayer x1back = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\backwards.wav");
            x1back.Play();
            MoveBackwards();
        }

        private void amuerte_Click(object sender, EventArgs e)
        {
            //System.Media.SoundPlayer muerte = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\muerte.wav");
            muerte.Play();

            AstronautHit();
        }
        #endregion

        //eventos de tipo click para items positivos que estan en desuso
        #region sin uso
        private void a1x_Click(object sender, EventArgs e) { }

        private void a4x_Click(object sender, EventArgs e) { }
        

        private void a2x_Click(object sender, EventArgs e) { }
        

        private void abug_Click(object sender, EventArgs e) { }
        #endregion
        #endregion

        //metodos que ejecutan acciones sobre los componentes cuando son llamadas por los eventos del programa
        #region Acciones

        
        private void MoveFordward1x()//mueve el astronauta 6 pixeles a la derecha
        {
            astronauta.Left += 6;
        }

        private void MoveFordward2x()//mueve el astronauta 12 pixeles a la derecha
        {
            astronauta.Left += 12;
        }

        private void MoveFordwardEnergize()//mueve el astronauta 25 pixeles a la derecha
        {
            astronauta.Left += 25;
        }

        private void PauseAstronauta()//activa la condicion de parada del astronauta
        {
            _pausaAstronauta = true;
        }

        private void MoveBackwards()//mueve el astronauta 12 pixeles hacia la izaquierda
        {
            astronauta.Left -= 12;
        }

        private void AlienBug()//activa la condicion de parada de los Malutos
        {
            _pausaMalutos = true;
        }

        private void AstronautHit()//has muerto porque patatas, se paran los hilos, se informa de la merte y se reinicia el programa
        {
            _pararHilos = true;
            MessageBox.Show("has muerto");
            Application.Restart();
        }

        //metodo de inicio de juego.Arranca los threads que procesaran las instrucciones relativas a los malos, al astronauta y a los pulsadores
        private void StartGame()
        {
            _malutoThread = new Thread(MetodoDelegado);
            _malutoThread.Start();
            _astronautaThread = new Thread(MetodoDelegado2);
            _astronautaThread.Start();
            _pulsadoresThread = new Thread(MetodoDelegado3);
            _pulsadoresThread.Start();

            

            //se ocultan el boton de iniciar juego, de seleccion de modo y la label informativa
            Jugar.Visible = false;
            SelectorModo.Visible = false;
            label1.Visible = false;

            //se muestran los malutos y el astronauta y se les determina la posicion inicial
            malutoRojo.Left = 0;
            malutoRojo.Visible = true;
            malutoAzul.Left = 0;
            malutoAzul.Visible = true;
            malutoVerde.Left = 0;
            malutoVerde.Visible = true;
            malutoNaranja.Left = 0;
            malutoNaranja.Visible = true;
            astronauta.Left = 387;
            astronauta.Visible = true;
            

        }

        //metodo que reproduce la musica que deberia ser de fondo e ir en un hilo independiente, pero que se detiene al inciar otro sonido. No alcancé a resolver este tema
        private void PlayMusicaBase()
        {
            //System.Media.SoundPlayer musicaBase = new System.Media.SoundPlayer(Application.StartupPath + @"\sonidos\musicaBase.wav");
            musicaBase.Play();
        }
        #endregion

    }
}
