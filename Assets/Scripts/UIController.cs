using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Create_Shape {
    public class UIController : MonoBehaviour {
        [SerializeField] private TMP_InputField _equationField;
        [SerializeField] private TMP_InputField _domainField1;
        [SerializeField] private TMP_InputField _domainField2;
        [SerializeField] private ModelMaker _modelMaker;
        [SerializeField] private Button _generateButton;

        private void Start() {
            _generateButton.onClick.AddListener(GenerateModel) ;
        }


        private void ValidateFloat(TMP_InputField field) {
        }
        private void GenerateModel() {
            var maineq = new Graph.EquationInput(_equationField.text);
            var yEq = new Graph.EquationInput("0");
            float left = float.Parse(_domainField1.text);
            float right = float.Parse(_domainField2.text);
            Vector2 domain = left < right ? new Vector2(left, right) : new Vector2(right, left);
            MeshType type = MeshType.Square;
            _modelMaker.Generate(maineq, yEq,domain,type);
        }
    }
}