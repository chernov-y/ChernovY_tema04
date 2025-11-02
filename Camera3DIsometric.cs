using OpenTK; // Avem nevoie de asta pentru structurile Vector3 și Matrix4
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL; // Avem nevoie de asta pentru funcțiile OpenGL (GL.MatrixMode, etc.)

namespace Tema_EGC
{
    /// <summary>
    /// Gestionează camera 3D. Deși e numită "Isometric",
    /// e de fapt o cameră de tip "LookAt" (privește spre o țintă)
    /// cu mișcări de translație (pan) pe axele lumii.
    /// </summary>
    class Camera3DIsometric
    {
        // --- Variabilele Membre ---

        // Cele trei componente esențiale ale unei camere "LookAt":
        private Vector3 eye;        // Poziția camerei (de unde privim)
        private Vector3 target;     // Punctul spre care privim (ținta)
        private Vector3 up_vector;  // Vectorul "sus" (de obicei (0, 1, 0) ca să nu fie camera rotită)

        // Cât de repede se mișcă camera la o singură apăsare de tastă.
        private const int MOVEMENT_UNIT = 1;

        // --- Constructori ---

        /// <summary>
        /// Constructorul implicit.
        /// Setează o poziție predefinită pentru cameră, de unde se vede bine scena.
        /// Valorile astea (200, 175, 25) care privesc spre (0, 25, 0)
        /// oferă o perspectivă bună "de sus și dintr-o parte".
        /// </summary>
        public Camera3DIsometric()
        {
            eye = new Vector3(200, 175, 25);
            target = new Vector3(0, 25, 0);
            up_vector = new Vector3(0, 1, 0); // (0, 1, 0) e "sus-ul" standard în grafică
        }

        // Următorii doi sunt constructori ajutători (overloads)
        // ca să putem crea o cameră cu valori proprii, dacă vrem,
        // fie trimițând numere simple (int), fie direct vectori (Vector3).

        public Camera3DIsometric(int _eyeX, int _eyeY, int _eyeZ, int _targetX, int _targetY, int _targetZ, int _upX, int _upY, int _upZ)
        {
            eye = new Vector3(_eyeX, _eyeY, _eyeZ);
            target = new Vector3(_targetX, _targetY, _targetZ);
            up_vector = new Vector3(_upX, _upY, _upZ);
        }

        public Camera3DIsometric(Vector3 _eye, Vector3 _target, Vector3 _up)
        {
            eye = _eye;
            target = _target;
            up_vector = _up;
        }

        // --- Metode Principale ---

        /// <summary>
        /// Aici se întâmplă magia. Metoda asta e chemată
        /// la început (în OnResize) și după fiecare mișcare a camerei.
        /// </summary>
        public void SetCamera()
        {
            // 1. Creăm matricea "View" (de vizualizare) folosind OpenTK.
            // Funcția LookAt calculează automat orientarea camerei
            // pe baza celor trei vectori: eye, target, și up.
            Matrix4 camera = Matrix4.LookAt(eye, target, up_vector);

            // 2. Spunem la OpenGL că vrem să modificăm matricea MODELVIEW.
            // (Matricea care se ocupă de transformările de model ȘI de vizualizare)
            GL.MatrixMode(MatrixMode.Modelview);

            // 3. Încărcăm matricea 'camera' pe care tocmai am calculat-o
            // în contextul OpenGL. De-acum, tot ce se desenează
            // va fi "văzut" din perspectiva acestei camere.
            GL.LoadMatrix(ref camera);
        }

        // --- METODE DE MIȘCARE ---
        //
        // Astea sunt funcțiile chemate din Window3D la apăsarea tastelor (W, A, S, D, Q, E).
        //
        // Notă importantă: Mișcarea se face prin translatarea
        // atât a 'ochiului' (eye) cât și a 'țintei' (target)
        // cu aceeași valoare, pe axele lumii (World Axes).
        // Asta e echivalentul unui "pan" (deplasare stânga-dreapta / sus-jos).

        public void MoveRight() // Folosită de tasta D
        {
            // Mutăm camera (eye) și ținta (target) pe axa Z negativă
            eye = new Vector3(eye.X, eye.Y, eye.Z - MOVEMENT_UNIT);
            target = new Vector3(target.X, target.Y, target.Z - MOVEMENT_UNIT);
            SetCamera(); // Trebuie să reaplicăm matricea ca să se vadă schimbarea!
        }
        public void MoveLeft() // Folosită de tasta A
        {
            // Mutăm camera pe axa Z pozitivă
            eye = new Vector3(eye.X, eye.Y, eye.Z + MOVEMENT_UNIT);
            target = new Vector3(target.X, target.Y, target.Z + MOVEMENT_UNIT);
            SetCamera();
        }

        public void MoveForward() // Folosită de tasta W
        {
            // Mutăm camera pe axa X negativă
            eye = new Vector3(eye.X - MOVEMENT_UNIT, eye.Y, eye.Z);
            target = new Vector3(target.X - MOVEMENT_UNIT, target.Y, target.Z);
            SetCamera();
        }

        public void MoveBackward() // Folosită de tasta S
        {
            // Mutăm camera pe axa X pozitivă
            eye = new Vector3(eye.X + MOVEMENT_UNIT, eye.Y, eye.Z);
            target = new Vector3(target.X + MOVEMENT_UNIT, target.Y, target.Z);
            SetCamera();
        }

        public void MoveUp() // Folosită de tasta Q
        {
            // Mutăm camera pe axa Y pozitivă (în sus)
            eye = new Vector3(eye.X, eye.Y + MOVEMENT_UNIT, eye.Z);
            target = new Vector3(target.X, target.Y + MOVEMENT_UNIT, target.Z);
            SetCamera();
        }

        public void MoveDown() // Folosită de tasta E
        {
            // Mutăm camera pe axa Y negativă (în jos)
            eye = new Vector3(eye.X, eye.Y - MOVEMENT_UNIT, eye.Z);
            target = new Vector3(target.X, target.Y - MOVEMENT_UNIT, target.Z);
            SetCamera();
        }

    }
}