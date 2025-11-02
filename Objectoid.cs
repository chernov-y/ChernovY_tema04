using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tema_EGC; // Include și clasa Randomizer

namespace Tema_EGC
{
    /// <summary>
    /// Reprezintă un obiect simplu (un "obiectoid" - probabil un cub)
    /// care poate fi generat aleatoriu și este afectat de gravitație.
    /// </summary>
    class Objectoid
    {
        // Flag-uri pentru starea obiectului
        private bool visibility;
        private bool isGravityBound; // Nefolosit în constructor, dar probabil era intenționat
        private Color colour; // Culoarea aleatorie a obiectului

        // Lista de vârfuri (colțuri) care definesc forma obiectului
        private List<Vector3> coordList;

        // O instanță a clasei noastre ajutătoare pentru a genera valori aleatorii
        private Randomizer rando;

        // Cât de mult "cade" obiectul la fiecare pas de update (frame de logică)
        private const int GRAVITY_OFFSET = 1;

        /// <summary>
        /// Constructorul. Este apelat când creăm un 'new Objectoid()' în Window3D.
        /// </summary>
        /// <param name="gravity_status">Primește starea gravitației din Window3D la momentul creării.</param>
        public Objectoid(bool gravity_status)
        {
            // Inițializăm generatorul de numere aleatorii
            rando = new Randomizer();

            // Setăm stările inițiale
            visibility = true; // Obiectul e vizibil de îndată ce e creat
            isGravityBound = gravity_status; // Preia starea gravitației (deși nu pare să fie folosită mai jos)
            colour = rando.RandomColor(); // Îi dăm o culoare aleatorie

            // --- Generarea formei obiectului ---
            coordList = new List<Vector3>();

            // Generăm niște valori aleatorii pentru a face fiecare obiect unic

            // 'size_offset': cât de mare e cubul (între 3 și 6)
            int size_offset = rando.RandomInt(3, 7);
            // 'height_offset': la ce înălțime apare (între 40 și 59)
            int height_offset = rando.RandomInt(40, 60);
            // 'radial_offset': cât de departe de origine (0,0) apare pe planul XZ (între 5 și 14)
            int radial_offset = rando.RandomInt(5, 15);

            // Aici se definesc vârfurile (colțurile) cubului.
            // Se folosesc 'size_offset' pentru a stabili dimensiunea cubului
            // și 'height_offset' + 'radial_offset' pentru a stabili poziția lui inițială.
            //
            // Lista asta de coordonate e specifică pentru a desena un cub
            // folosind 'PrimitiveType.QuadStrip'. Un QuadStrip leagă vârfurile
            // într-o bandă continuă de patrulatere (fețe).
            //
            // Ex: v0, v1, v2, v3 formează primul patrulater (față)
            //     v2, v3, v4, v5 formează al doilea patrulater
            //     v4, v5, v6, v7 formează al treilea, etc.

            // Această listă definește cele 8 vârfuri ale unui cub, plus 2 vârfuri
            // repetate la final pentru a închide corect QuadStrip-ul.

            // Baza de jos (Y = 0 * size_offset + height_offset)
            coordList.Add(new Vector3(0 * size_offset + radial_offset, 0 * size_offset + height_offset, 1 * size_offset + radial_offset)); // v0
            coordList.Add(new Vector3(0 * size_offset + radial_offset, 0 * size_offset + height_offset, 0 * size_offset + radial_offset)); // v1
            coordList.Add(new Vector3(1 * size_offset + radial_offset, 0 * size_offset + height_offset, 1 * size_offset + radial_offset)); // v2
            coordList.Add(new Vector3(1 * size_offset + radial_offset, 0 * size_offset + height_offset, 0 * size_offset + radial_offset)); // v3

            // "Capacul" de sus (Y = 1 * size_offset + height_offset)
            coordList.Add(new Vector3(1 * size_offset + radial_offset, 1 * size_offset + height_offset, 1 * size_offset + radial_offset)); // v4
            coordList.Add(new Vector3(1 * size_offset + radial_offset, 1 * size_offset + height_offset, 0 * size_offset + radial_offset)); // v5
            coordList.Add(new Vector3(0 * size_offset + radial_offset, 1 * size_offset + height_offset, 1 * size_offset + radial_offset)); // v6
            coordList.Add(new Vector3(0 * size_offset + radial_offset, 1 * size_offset + height_offset, 0 * size_offset + radial_offset)); // v7

            // Repetăm primele două vârfuri de sus pentru a închide "banda"
            coordList.Add(new Vector3(0 * size_offset + radial_offset, 0 * size_offset + height_offset, 1 * size_offset + radial_offset)); // v8 (identic cu v0)
            coordList.Add(new Vector3(0 * size_offset + radial_offset, 0 * size_offset + height_offset, 0 * size_offset + radial_offset)); // v9 (identic cu v1)
        }

        /// <summary>
        /// Metoda de desenare. Chemată din 'OnRenderFrame' (din Window3D).
        /// </summary>
        public void Draw()
        {
            // Desenăm doar dacă obiectul e vizibil.
            if (visibility)
            {
                GL.Color3(colour); // Setăm culoarea aleasă pentru acest obiect

                // Începem desenarea folosind QuadStrip.
                // Asta înseamnă că OpenGL va lua vârfurile din listă
                // și le va uni în fețe de câte 4.
                GL.Begin(PrimitiveType.QuadStrip);

                // Trimitem fiecare vârf din lista noastră către placa video
                foreach (Vector3 vector in coordList)
                {
                    GL.Vertex3(vector);
                }

                GL.End(); // Am terminat de desenat
            }
        }

        /// <summary>
        /// Metoda de logică. Chemată din 'OnUpdateFrame' (din Window3D).
        /// Aici simulăm gravitația.
        /// </summary>
        public void UpdatePosition(bool gravity_status)
        {
            // Obiectul se mișcă (cade) doar dacă:
            // 1. E vizibil
            // 2. Gravitația e activată la nivel global (parametrul 'gravity_status')
            // 3. NU a detectat încă o coliziune cu solul (Y=0)
            if (visibility && gravity_status && !GroundCollisionDetected())
            {
                // Parcurgem TOATE vârfurile din listă...
                for (int i = 0; i < coordList.Count; i++)
                {
                    // ...și le modificăm poziția.
                    // Creăm un nou Vector3, păstrând X și Z,
                    // dar scăzând din Y valoarea 'GRAVITY_OFFSET'.
                    // Asta face ca obiectul să "cadă" cu 1 unitate pe axa Y.
                    coordList[i] = new Vector3(coordList[i].X, coordList[i].Y - GRAVITY_OFFSET, coordList[i].Z);
                }
            }
        }

        /// <summary>
        /// O metodă ajutătoare care verifică dacă vreunul din vârfurile
        /// obiectului a atins sau a trecut de "podea" (Y=0).
        /// </summary>
        public bool GroundCollisionDetected()
        {
            // Verificăm fiecare vârf
            foreach (Vector3 vector in coordList)
            {
                // Dacă găsim MĂCAR UN vârf care are Y <= 0...
                if (vector.Y <= 0)
                {
                    // ...înseamnă că am lovit pământul.
                    return true;
                }
            }

            // Dacă am verificat toate vârfurile și niciunul nu e sub Y=0,
            // înseamnă că încă suntem în aer.
            return false;
        }

        /// <summary>
        /// Metodă publică pentru a schimba vizibilitatea (folosită de 'O' în Window3D,
        /// deși în codul tău 'O' afectează 'objy', nu 'rainOfObjects').
        /// </summary>
        public void ToggleVisibility()
        {
            visibility = !visibility;
        }
    }
}