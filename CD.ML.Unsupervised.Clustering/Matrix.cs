using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.ML.Unsupervised.Clustering {

    public class Matrix<T> {

        private int _rows = 0;
        private int _cols = 0;
        private T[][] _data = null;

        public Matrix() { }

        public Matrix(T[][] data) {
            _rows = data.Length;
            if (_rows > 0)
                _cols = data[0].Length;
            Validate();

            _data = data;
        }

        public Matrix(int rows, int cols) {
            _rows = rows;
            _cols = cols;

            Validate(); 

            _data = new T[rows][];
            for (int r = 0; r < rows; r++)
                _data[r] = new T[cols];
        }

        public int Count {
            get {
                return _rows;
            }
        }

        public int FeatureCount {
            get {
                return _cols;
            }
        }

        public bool IsInitialized {
            get {
                return !(_data != null);
            }
        }

        // indexer 
        public T[] this[int index] {
            get {
                return _data[index];
            }
        }

        private void Validate() {
            if (_rows <= 0)
                throw new ArgumentOutOfRangeException("Row Count", _rows, "Row count must be greater than zero.");
            if (_cols <= 0)
                throw new ArgumentOutOfRangeException("Column Count", _cols, "Column count must be greater than zero.");
        }
    }
}
