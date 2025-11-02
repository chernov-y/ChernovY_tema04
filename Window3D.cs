using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input; // Namespace-ul pentru Keyboard și Mouse
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Tema; // Comentat, pare a fi un namespace vechi sau nefolosit
using Tema_EGC;

namespace Tema_EGC
{
    /// <summary>
    /// Fereastra grafică principală. Moștenește din clasa 'GameWindow' a OpenTK.
    /// Aceasta ne oferă automat un "game loop" cu OnLoad, OnUpdateFrame și OnRenderFrame.
    /// </summary>
    class Window3D : GameWindow
    {
        // --- Variabile Membre (Starea aplicației) ---

        // Avem nevoie de starea tastaturii și a mouse-ului de la frame-ul anterior
        // ca să putem detecta o *apăsare* de tastă (vs. o ținere apăsată).
        private KeyboardState previousKeyboard;
        private MouseState previousMouse;

        // Obiectele noastre pe care le vom desena în scenă.
        // Sunt 'readonly' pentru că le inițializăm o singură dată în constructor.
        private readonly Randomizer rando;
        private readonly Axes ax;
        private readonly Grid grid;
        private readonly Camera3DIsometric cam;
        private MassiveObject objy; // Obiectul .obj încărcat din fișier

        // O listă pentru a ține minte toate obiectele "Objectoid" (cuburile)
        // pe care le creăm cu click stânga.
        private List<Objectoid> rainOfObjects;
        // Un flag global care ne spune dacă gravitația e pornită sau nu.
        private bool GRAVITY = true;

        // Contoare pentru debugging/info
        private bool displayMarker;
        private ulong updatesCounter;
        private ulong framesCounter;

        // Culoarea de fundal implicită (un gri închis).
        private readonly Color DEFAULT_BKG_COLOR = Color.FromArgb(49, 50, 51);


        /// <summary>
        /// Constructorul ferestrei.
        /// Aici se fac toate inițializările ("inits").
        /// </summary>
        public Window3D() : base(1280, 768, new GraphicsMode(32, 24, 0, 8))
        // 'base(...)' setează titlul, rezoluția și modul grafic.
        // 'new GraphicsMode(32, 24, 0, 8)' cere:
        // 32 biți culoare, 24 biți buffer de adâncime (depth buffer), 0 biți stencil, 8 sample-uri anti-aliasing (MSAA)
        {
            // VSync-ul sincronizează rata de cadre (FPS) cu rata de reîmprospătare a monitorului.
            // Previne "screen tearing" (ruperea imaginii).
            VSync = VSyncMode.On;

            // --- Inițializări ---
            // Creăm instanțe noi pentru toate obiectele noastre ajutătoare.
            rando = new Randomizer();
            ax = new Axes();
            grid = new Grid();
            cam = new Camera3DIsometric();
            objy = new MassiveObject(Color.Yellow); // Creăm obiectul .obj și îi dăm o culoare

            // Inițializăm lista de obiecte. La început e goală.
            rainOfObjects = new List<Objectoid>();

            // Afișăm meniul în consolă de îndată ce pornește programul.
            DisplayHelp();
            displayMarker = false; // Flag-ul de debug e oprit inițial
            updatesCounter = 0;
            framesCounter = 0;
        }

        /// <summary>
        /// Se execută O SINGURĂ DATĂ, la pornirea aplicației, după ce contextul OpenGL a fost creat.
        /// Perfect pentru a seta stări OpenGL care nu se schimbă.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // --- Setări Globale OpenGL ---

            // Activăm testul de adâncime (Depth Test).
            // Asta e VITAL pentru 3D. Face ca obiectele din față
            // să le acopere pe cele din spate. Fără el, totul e un haos.
            GL.Enable(EnableCap.DepthTest);
            // Specifică funcția de testare: un pixel nou îl acoperă pe cel vechi doar dacă e *mai aproape* (Less).
            GL.DepthFunc(DepthFunction.Less);

            // O setare de calitate (hint) pentru placa video,
            // îi cerem să facă anti-aliasing la poligoane cât mai frumos.
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
        }

        /// <summary>
        /// Se execută cel puțin o dată (după OnLoad) și de fiecare dată când fereastra își schimbă dimensiunea.
        /// Aici setăm viewport-ul și matricea de Proiecție (perspectiva).
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Setăm culoarea cu care se va "șterge" ecranul la fiecare cadru.
            GL.ClearColor(DEFAULT_BKG_COLOR);

            // Setăm viewport-ul. Spunem OpenGL să deseneze pe toată suprafața ferestrei.
            GL.Viewport(0, 0, this.Width, this.Height);

            // --- Setarea Proiecției (Perspectivei) ---
            // Asta e ca și cum am alege "obiectivul camerei" (ex: wide-angle, telephoto).

            // 1. Creăm matricea de perspectivă.
            // MathHelper.PiOver4 = 45 de grade (câmpul vizual, FOV)
            // (float)this.Width / (float)this.Height = aspect ratio (raportul lățime/înălțime al ferestrei)
            // 1 = near clipping plane (cât de aproape vedem)
            // 1024 = far clipping plane (cât de departe vedem)
            Matrix4 perspectiva = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)this.Width / (float)this.Height, 1, 1024);

            // 2. Spunem OpenGL că vrem să modificăm matricea PROJECTION.
            GL.MatrixMode(MatrixMode.Projection);

            // 3. Încărcăm matricea 'perspectiva' în OpenGL.
            GL.LoadMatrix(ref perspectiva);

            // 4. Setăm camera (matricea Modelview).
            // Asta stabilește de *unde* privim și *încotro* (eye, target, up).
            cam.SetCamera();
        }

        /// <summary>
        /// Se execută de mai multe ori pe secundă (ex: 30 FPS, setat în Program.cs).
        /// Aici se pune TOATĂ LOGICA: input, fizică, mișcare, coliziuni etc.
        /// NU se desenează nimic aici!
        /// </summary>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            updatesCounter++;

            if (displayMarker)
            {
                TimeStampIt("update", updatesCounter.ToString());
            }

            // --- LOGIC CODE ---
            // Citim starea curentă a tastaturii și mouse-ului
            KeyboardState currentKeyboard = Keyboard.GetState();
            MouseState currentMouse = Mouse.GetState();

            // --- Management Input Tastă ---

            // Tasta ESC: închide aplicația
            if (currentKeyboard[Key.Escape])
            {
                Exit();
            }

            // Detectare *apăsare* (nu ținere apăsată)
            // Logica e: tasta e apăsată ACUM (current) ȘI NU era apăsată în frame-ul trecut (previous)?

            // Tasta H: Afișează meniul
            if (currentKeyboard[Key.H] && !previousKeyboard[Key.H])
            {
                DisplayHelp();
            }

            // Tasta R: Resetează scena
            if (currentKeyboard[Key.R] && !previousKeyboard[Key.R])
            {
                GL.ClearColor(DEFAULT_BKG_COLOR); // Resetăm fundalul
                ax.Show(); // Aratăm axele
                grid.Show(); // Aratăm grila
            }

            // Tasta K: Comută vizibilitatea axelor
            if (currentKeyboard[Key.K] && !previousKeyboard[Key.K])
            {
                ax.ToggleVisibility();
            }

            // Tasta B: Schimbă fundalul cu o culoare aleatorie
            if (currentKeyboard[Key.B] && !previousKeyboard[Key.B])
            {
                GL.ClearColor(rando.RandomColor());
            }

            // Tasta V: Comută vizibilitatea grilei
            if (currentKeyboard[Key.V] && !previousKeyboard[Key.V])
            {
                grid.ToggleVisibility();
            }

            // Tasta O: Comută vizibilitatea obiectului .obj
            if (currentKeyboard[Key.O] && !previousKeyboard[Key.O])
            {
                objy.ToggleVisibility();
            }

            // --- Control Cameră (Ținere Apăsată) ---
            // Aici NU verificăm 'previousKeyboard' pentru că vrem
            // ca mișcarea să fie continuă cât timp ținem tasta apăsată.
            if (currentKeyboard[Key.W])
            {
                cam.MoveForward();
            }
            if (currentKeyboard[Key.S])
            {
                cam.MoveBackward();
            }
            if (currentKeyboard[Key.A])
            {
                cam.MoveLeft();
            }
            if (currentKeyboard[Key.D])
            {
                cam.MoveRight();
            }
            if (currentKeyboard[Key.Q])
            {
                cam.MoveUp();
            }
            if (currentKeyboard[Key.E])
            {
                cam.MoveDown();
            }

            // Tasta L: Comută marker-ul de debug
            if (currentKeyboard[Key.L] && !previousKeyboard[Key.L])
            {
                displayMarker = !displayMarker;
            }

            // --- Management Input Mouse ---

            // Click stânga: generează un obiect nou
            if (currentMouse[MouseButton.Left] && !previousMouse[MouseButton.Left])
            {
                // Adăugăm un nou 'Objectoid' în lista noastră.
                // Îi dăm starea curentă a gravitației.
                rainOfObjects.Add(new Objectoid(GRAVITY));
            }

            // Click dreapta: curăță toate obiectele
            if (currentMouse[MouseButton.Right] && !previousMouse[MouseButton.Right])
            {
                rainOfObjects.Clear();
            }

            // --- Logica Jocului ---

            // Tasta G: Comută gravitația
            if (currentKeyboard[Key.G] && !previousKeyboard[Key.G])
            {
                GRAVITY = !GRAVITY; // Inversează valoarea (true -> false, false -> true)
            }

            // Actualizează poziția fiecărui obiect care cade
            // Trecem prin fiecare obiect din listă...
            foreach (Objectoid obj in rainOfObjects)
            {
                // ...și apelăm metoda lui de Update, trimițându-i starea gravitației.
                obj.UpdatePosition(GRAVITY);
            }

            // Foarte important: la finalul frame-ului de logică,
            // salvăm starea curentă a input-ului ca fiind starea "veche"
            // pentru frame-ul următor.
            previousKeyboard = currentKeyboard;
            previousMouse = currentMouse;
            // --- END logic code ---
        }

        /// <summary>
        /// Se execută cât de repede poate placa video (sau e limitat de VSync).
        /// Aici se pune DOAR codul de desenare (Render).
        /// NU se face logică (input, fizică) aici!
        /// </summary>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            framesCounter++;

            if (displayMarker)
            {
                TimeStampIt("render", framesCounter.ToString());
            }

            // --- Ștergerea Ecranului ---
            // Trebuie să ștergem două buffere înainte de a desena noul cadru:

            // 1. ColorBufferBit: Șterge culorile de pe ecran (umple cu 'ClearColor').
            GL.Clear(ClearBufferMask.ColorBufferBit);
            // 2. DepthBufferBit: Șterge informațiile de adâncime. E vital!
            // Altfel, obiectele din frame-ul trecut ar "bloca" pixelii.
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // --- RENDER CODE ---
            // Ordinea desenării contează!
            // De obicei desenăm de la "spate" la "față" sau
            // întâi obiectele opace, apoi cele transparente.

            // 1. Desenează grila (podeaua)
            grid.Draw();
            // 2. Desenează axele
            ax.Draw();
            // 3. Desenează obiectul .obj principal
            objy.Draw();

            // 4. Desenează TOATE obiectele din lista 'rainOfObjects'
            foreach (Objectoid obj in rainOfObjects)
            {
                obj.Draw();
            }
            // --- END render code ---

            // --- Double Buffering ---
            // Tot ce am desenat până acum (GL.Begin, GL.Vertex, etc.)
            // s-a desenat într-un "buffer" (o imagine) ascuns.
            // 'SwapBuffers()' ia imaginea ascunsă, complet desenată,
            // și o afișează dintr-o dată pe ecran.
            // Asta previne pâlpâirea (flickering).
            SwapBuffers();
        }

        /// <summary>
        /// Metodă ajutătoare (helper) care afișează meniul în consolă.
        /// </summary>
        private void DisplayHelp()
        {
            Console.WriteLine("\n      MENIU");
            Console.WriteLine(" (H) - meniul");
            Console.WriteLine(" (ESC) - parasire aplicatie");
            Console.WriteLine(" (K) - schimbare vizibilitate sistem de axe");
            Console.WriteLine(" (R) - resteaza scena la valori implicite");
            Console.WriteLine(" (B) - schimbare culoare de fundal");
            Console.WriteLine(" (V) - schimbare vizibilitate linii");
            Console.WriteLine(" (W,A,S,D,Q,E) - deplasare camera (pan)"); // Am corectat, e 'pan' nu 'izometric'
            Console.WriteLine(" (O) - comuta vizibilitate obiect .obj"); // Am adăugat eu asta, bazat pe cod
            Console.WriteLine(" (G) - manipuleaza gravitatea");
            Console.WriteLine(" (Mouse click stanga) - genereaza un obiect nou la o inaltime aleatoare");
            Console.WriteLine(" (Mouse click dreapta) - curata lista de obiecte");
        }

        /// <summary>
        /// Metodă ajutătoare pentru debug, afișează un timestamp în consolă.
        /// </summary>
        private void TimeStampIt(String source, String counter)
        {
            String dt = DateTime.Now.ToString("hh:mm:ss.ffff");
            Console.WriteLine("     TSTAMP from <" + source + "> on iteration <" + counter + ">: " + dt);
        }

    }
}