using System;
using System.Collections.Generic;
using System.Drawing; // Avem nevoie de asta pentru a folosi tipul 'Color'
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL; // Importăm funcțiile de bază OpenGL (GL.Begin, GL.Vertex3, etc.)

namespace Tema_EGC
{
    /// <summary>
    /// Această clasă desenează o grilă pe planul XZ (adică "podeaua" scenei, la Y=0).
    /// Ne ajută să ne orientăm în spațiul 3D.
    /// </summary>
    class Grid
    {
        // Culoarea pe care o va avea grila. E 'readonly' pentru că o setăm
        // o singură dată în constructor și nu se mai schimbă.
        private readonly Color colorisation;

        // Un flag boolean simplu. Dacă e 'true', grila se desenează.
        // Dacă e 'false', metoda Draw() nu va face nimic.
        private bool visibility;

        // --- CONSTANTE ---
        // Aici definim cum arată grila.

        // Culoarea implicită a grilei.
        private readonly Color GRIDCOLOR = Color.WhiteSmoke;
        // Distanța dintre două linii paralele ale grilei (cât de dese sunt pătratele).
        private const int GRIDSTEP = 10;
        // Câte "unități" (linii) să desenăm în fiecare direcție (pozitiv/negativ) pornind de la centru.
        private const int UNITS = 50;
        // Calculăm automat cât de întinsă e grila.
        // În acest caz, 10 * 50 = 500. Grila se va întinde de la -500 la +500 pe X și Z.
        private const int POINT_OFFSET = GRIDSTEP * UNITS;

        // Un offset (decalaj) foarte mic, de 1 unitate.
        // Comentariul original explică de ce: e folosit ca să nu se suprapună grila
        // perfect cu axele (care sunt desenate tot la Y=0). Altfel, ar apărea
        // artefacte vizuale (z-fighting) unde liniile "se bat" pentru care să fie afișată.
        private const int MICRO_OFFSET = 1;

        /// <summary>
        /// Constructorul clasei. E apelat când creăm un obiect de tip 'Grid' (în Window3D).
        /// </summary>
        public Grid()
        {
            // Setăm culoarea grilei la valoarea noastră constantă.
            colorisation = GRIDCOLOR;
            // Când pornește aplicația, vrem ca grila să fie vizibilă.
            visibility = true;
        }

        // --- Metode simple pentru a controla vizibilitatea din exterior (din Window3D) ---

        public void Show()
        {
            visibility = true;
        }

        public void Hide()
        {
            visibility = false;
        }

        /// <summary>
        /// Inversează starea de vizibilitate. 
        /// (true devine false, false devine true).
        /// Asta folosim de obicei ca să o legăm de o tastă.
        /// </summary>
        public void ToggleVisibility()
        {
            visibility = !visibility;
        }

        /// <summary>
        /// Metoda care desenează efectiv grila. 
        /// E chemată în mod repetat din 'OnRenderFrame' (din Window3D).
        /// </summary>
        public void Draw()
        {
            // Verificăm mai întâi dacă grila e setată să fie vizibilă.
            // Dacă nu e, metoda se încheie și nu desenează nimic.
            if (visibility)
            {
                // Spunem placii video că începem să desenăm o serie de Linii.
                // Fiecare pereche de GL.Vertex3() va fi o linie.
                GL.Begin(PrimitiveType.Lines);

                // Setăm culoarea pentru TOATE liniile pe care le vom desena acum.
                GL.Color3(colorisation);

                // Folosim un 'for' ca să desenăm toate liniile paralele.
                // 'i' va lua valori: -500, -490, -480, ..., 0, ..., 490, 500.
                for (int i = -1 * GRIDSTEP * UNITS; i <= GRIDSTEP * UNITS; i += GRIDSTEP)
                {
                    // Pentru fiecare valoare a lui 'i', desenăm 2 linii:

                    // 1. O linie paralelă cu axa Oz (axa Z).
                    // X-ul este constant (la valoarea 'i'), iar Z-ul variază de la +500 la -500.
                    // Adăugăm acel 'MICRO_OFFSET' la X.
                    GL.Vertex3(i + MICRO_OFFSET, 0, POINT_OFFSET);
                    GL.Vertex3(i + MICRO_OFFSET, 0, -1 * POINT_OFFSET);

                    // 2. O linie paralelă cu axa Ox (axa X).
                    // Z-ul este constant (la valoarea 'i'), iar X-ul variază de la +500 la -500.
                    // Adăugăm acel 'MICRO_OFFSET' la Z.
                    GL.Vertex3(POINT_OFFSET, 0, i + MICRO_OFFSET);
                    GL.Vertex3(-1 * POINT_OFFSET, 0, i + MICRO_OFFSET);
                }

                // Am terminat de trimis toate vârfurile. Spunem placii video să încheie desenarea.
                GL.End();
            }
        }
    }
}