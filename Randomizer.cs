using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing; // Avem nevoie de asta pentru 'Color'
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tema_EGC
{
    /// <summary>
    /// Clasa asta e un "ajutor" (helper class).
    /// Rolul ei e să ne dea rapid diverse valori aleatorii (random)
    /// ca să nu trebuiască să scriem 'new Random().Next(...)' peste tot prin cod.
    /// </summary>
    class Randomizer
    {
        // Aici ținem instanța generatorului de numere.
        // O creăm o singură dată în constructor.
        private Random r;

        // --- Constante pentru intervale implicite ---
        // Astea sunt folosite de metodele care nu primesc parametri,
        // ca să avem niște valori standard.
        private const int LOW_INT_VAL = -25;
        private const int HIGH_INT_VAL = 25;
        private const int LOW_COORD_VAL = -50;
        private const int HIGH_COORD_VAL = 50;

        /// <summary>
        /// Constructorul standard.
        /// </summary>
        public Randomizer()
        {
            // Inițializăm generatorul. Când e chemat fără un "seed" (sămânță),
            // 'new Random()' folosește ceasul sistemului.
            // Asta asigură că de fiecare dată când pornim programul,
            // vom avea o altă secvență de numere aleatorii.
            r = new Random();
        }

        /// <summary>
        /// Metodă care ne dă o culoare aleatorie.
        /// </summary>
        public Color RandomColor()
        {
            // 'r.Next(0, 255)' va genera un număr între 0 și 254.
            // Pentru culori RGB, intervalul e 0-255.
            // Corect ar fi fost 'r.Next(0, 256)' sau 'r.Next(256)'
            // ca să includă și 255, dar diferența e minoră.
            int genR = r.Next(0, 255); // Componenta Roșu
            int genG = r.Next(0, 255); // Componenta Verde
            int genB = r.Next(0, 255); // Componenta Albastru

            // Creăm culoarea folosind cele 3 componente generate.
            Color col = Color.FromArgb(genR, genG, genB);

            return col;
        }

        /// <summary>
        /// Metodă care ne dă un punct 3D (Vector3) aleatoriu.
        /// Folosește intervalul predefinit LOW/HIGH_COORD_VAL.
        /// </summary>
        public Vector3 Random3DPoint()
        {
            // Generează 3 numere (pentru x, y, z)
            // în intervalul [-50, 50) (adică de la -50 inclusiv până la 49 inclusiv)
            int genA = r.Next(LOW_COORD_VAL, HIGH_COORD_VAL);
            int genB = r.Next(LOW_COORD_VAL, HIGH_COORD_VAL);
            int genC = r.Next(LOW_COORD_VAL, HIGH_COORD_VAL);

            // Creăm un vector 3D cu valorile generate.
            Vector3 vec = new Vector3(genA, genB, genC);

            return vec;
        }

        // --- Suprapuneri (Overloads) pentru RandomInt ---
        // Avem 3 funcții cu același nume ('RandomInt'), dar cu parametri diferiți.
        // Asta se numește "suprapunerea metodelor" (method overloading).

        /// <summary>
        /// Returnează un int aleatoriu folosind intervalul standard (ex: [-25, 25)).
        /// </summary>
        public int RandomInt()
        {
            // Folosește constantele definite la începutul clasei.
            int i = r.Next(LOW_INT_VAL, HIGH_INT_VAL);

            return i;
        }

        /// <summary>
        /// Returnează un int aleatoriu într-un interval specificat de noi.
        /// </summary>
        /// <param name="minVal">Valoarea minimă (inclusiv)</param>
        /// <param name="maxVal">Valoarea maximă (exclusiv)</param>
        public int RandomInt(int minVal, int maxVal)
        {
            // Asta e metoda de bază 'Next' din 'Random'.
            // Generează un număr 'i' unde minVal <= i < maxVal.
            int i = r.Next(minVal, maxVal);

            return i;
        }

        /// <summary>
        /// Returnează un int aleatoriu între 0 și o valoare maximă dată.
        /// </summary>
        /// <param name="maxval">Valoarea maximă (exclusiv)</param>
        public int RandomInt(int maxval)
        {
            // E un shortcut pentru 'r.Next(0, maxval)'.
            // Generează un număr 'i' unde 0 <= i < maxval.
            int i = r.Next(maxval);

            return i;
        }
    }
}