using System.Collections.Generic;

namespace DataTrack.Data
{
    public class Kernel
    {
        private readonly List<Material> _materials;
        private readonly List<InputTanker> _inputTankers;
        private readonly List<Silos> _siloses;
        private readonly List<Conveyor> _conveyors;

        public Kernel()
        {
            _materials = new List<Material>();
            _inputTankers = new List<InputTanker>();
            _siloses = new List<Silos>();
            _conveyors = new List<Conveyor>();
        }

        /// <summary>
        /// Добавить новый материал
        /// </summary>
        /// <param name="material">Добавляемый материал</param>
        public void AddMaterial(Material material)
        {
            if (material != null)
            {
                _materials.Add(material);
            }
        }

        /// <summary>
        /// Получить список всех материалов
        /// </summary>
        /// <returns>Список материалов</returns>
        public List<Material> GetMaterials()
        {
            return _materials;
        }

        /// <summary>
        /// Получить количество материалов в списке
        /// </summary>
        /// <returns>Количество материалов в списке</returns>
        public int GetMaterialsCount()
        {
            return _materials.Count;
        }

        /// <summary>
        /// Получить материал из списка по его номеру
        /// </summary>
        /// <param name="num">Номер материала</param>
        /// <returns>Материал из списка</returns>
        public Material GetMaterial(int num)
        {
            Material result = null;
            if (num > 0 && num <= _materials.Count)
            {
                result = _materials[num - 1];
            }

            return result;
        }
    }
}