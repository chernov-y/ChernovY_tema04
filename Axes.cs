using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL; // Avem nevoie de funcțiile OpenGL (GL.Begin, GL.Color3, etc.)
using System.Drawing; // Avem nevoie de 'Color' pentru a defini culorile axelor

namespace Tema_EGC
{
    /// <summary>
    /// Această clasă desenează un sistem de coordonate XYZ simplu (axele X, Y, Z).
    /// Este foarte util pentru a ne orienta în scena 3D,
    /// ca să știm care direcție e X, care e Y și care e Z.
    /// </summary>
    class Axes
    {
        // Un flag boolean care controlează dacă axele sunt desenate sau nu.
        private bool myVisibility;

        // O constantă care definește cât de lungă să fie fiecare axă,
        // pornind de la origine (0, 0, 0).
        private const int AXIS_LENGTH = 75;

        /// <summary>
        /// Constructorul clasei.
        /// </summary>
        public Axes()
        {
            // Când creăm obiectul 'Axes', setăm vizibilitatea ca fiind 'true'
            // (vrem să vedem axele de la început).
            myVisibility = true;
        }

        /// <summary>
        /// Metoda principală de desenare.
        /// Trebuie chemată din 'OnRenderFrame' (din Window3D).
        /// </summary>
        public void Draw()
        {
            // Verificăm dacă flag-ul de vizibilitate este 'true'.
            // Dacă nu, metoda se oprește aici și nu desenează nimic.
            if (myVisibility)
            {
                // Setăm grosimea liniei la 1 pixel (valoarea standard).
                // E o practică bună s-o setezi, deși s-ar putea să nu fie neapărat necesar.
                GL.LineWidth(1.0f);

                // Spunem placii video că începem să desenăm o serie de Linii.
                // Fiecare 2 apeluri 'GL.Vertex3' vor forma o linie.
                GL.Begin(PrimitiveType.Lines);

                // --- Axa X (Roșu) ---
                GL.Color3(Color.Red); // Setăm culoarea curentă la Roșu
                GL.Vertex3(0, 0, 0); // Punctul de start al liniei (originea)
                GL.Vertex3(AXIS_LENGTH, 0, 0); // Punctul de final (pe axa X)

                // --- Axa Y (Verde) ---
                GL.Color3(Color.ForestGreen); // Setăm culoarea curentă la Verde
                GL.Vertex3(0, 0, 0); // Punctul de start (originea)
                GL.Vertex3(0, AXIS_LENGTH, 0); // Punctul de final (pe axa Y)

                // --- Axa Z (Albastru) ---
                GL.Color3(Color.RoyalBlue); // Setăm culoarea curentă la Albastru
                GL.Vertex3(0, 0, 0); // Punctul de start (originea)
                GL.Vertex3(0, 0, AXIS_LENGTH); // Punctul de final (pe axa Z)

                // Am terminat de trimis toate cele 3 linii (6 vârfuri).
                GL.End();
            }
        }

        // --- Metode ajutătoare pentru controlul vizibilității ---
        // Acestea sunt apelate din 'Window3D' ca răspuns la apăsarea tastelor.

        /// <summary>
        /// Setează vizibilitatea pe ON (vizibil).
        /// </summary>
        public void Show()
        {
            myVisibility = true;
        }

        /// <summary>
        /// Setează vizibilitatea pe OFF (ascuns).
        /// </summary>
        public void Hide()
        {
            myVisibility = false;
        }

        /// <summary>
        /// Inversează starea de vizibilitate (ON -> OFF sau OFF -> ON).
        /// Aceasta este metoda folosită de obicei pentru tasta 'K'.
        /// </summary>
        public void ToggleVisibility()
        {
            myVisibility = !myVisibility;
        }
    }
}