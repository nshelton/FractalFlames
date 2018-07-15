using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class THREETrackballCamera : MonoBehaviour
{

    private enum STATE
    {
        NONE = -1,
        ROTATE = 0,
        PAN = 1,
        ZOOM = 2,
        TOUCH_ROTATE = 3,
        TOUCH_ZOOM_PAN = 4
    }

    [SerializeField]
    private float rotateSpeed = 1;

    [SerializeField]
    private float zoomSpeed = 1;

    [SerializeField]
    private float panSpeed = 0.3f;

    [SerializeField]
    private float m_flySensitivity = 0.3f;




    [SerializeField]
    private bool noRotate = false;

    [SerializeField]
    private bool noZoom = false;

    [SerializeField]
    private bool noPan = false;


    [SerializeField]
    private bool staticMoving = false;

    [SerializeField]
    private float dynamicDampingFactor = 0.2f;


    [SerializeField]
    private float minDistance = 0;

    [SerializeField]
    private float maxDistance = float.MaxValue;


    // this.keys = [65 /*A*/, 83 /*S*/, 68 /*D*/ ];

    // internals

    [SerializeField]
    private Transform target;


    [SerializeField]
    private Transform ball;

    private float EPS = 0.000001f;

    private Vector3 lastPosition;

    private STATE _state = STATE.NONE;
    private STATE _prevState = STATE.NONE;

    private Vector3 _eye;
    private Vector3 _up;

    private Vector2 _movePrev;
    private Vector2 _moveCurr;

    private Vector3 _lastAxis;
    private float _lastAngle;

    private Vector2 _zoomStart;
    private Vector2 _zoomEnd;

    private float _touchZoomDistanceStart;
    private float _touchZoomDistanceEnd;

    private Vector2 _panStart;
    private Vector2 _panEnd;

    // for reset
    private Transform target0; // = target.clone();

    private Vector3 position0; // = object.position.clone();

    private Vector3 up0; // = object.up.clone();

    // events

    private Vector2 normalizedMouseCoord(int pageX, int pageY)
    {

        // [0,1]x[0,1]
        return new Vector2(
                (float)pageX / (float)Screen.width,
                (float)pageY / (float)Screen.height
            );
    }

    private Vector2 getMouseOnCircle(int pageX, int pageY)
    {
        pageX = Screen.width - pageX;
        pageY = Screen.height - pageY;

        return new Vector2(
            ((pageX - Screen.width * 0.5f) / (Screen.width * 0.5f)),
            ((Screen.height + 2f * (Screen.width - pageY)) / Screen.width) // Screen.width intentional
        );

    }
    private void rotateCamera()
    {

        var axis = new Vector3();
        var quaternion = new Quaternion();
        var eyeDirection = new Vector3();
        var objectUpDirection = new Vector3();
        var objectSidewaysDirection = new Vector3();
        var moveDirection = new Vector3();
        var angle = 0f;

        moveDirection.Set(_moveCurr.x - _movePrev.x, _moveCurr.y - _movePrev.y, 0);
        angle = moveDirection.magnitude;

        if (angle > 0f)
        {

            _eye = (transform.position) - (target.position);
            eyeDirection = _eye.normalized;
            objectUpDirection = (transform.up).normalized;

            objectSidewaysDirection = Vector3.Cross(objectUpDirection, eyeDirection).normalized;

            objectUpDirection = objectUpDirection.normalized * (_moveCurr.y - _movePrev.y);
            objectSidewaysDirection = objectSidewaysDirection.normalized * (_moveCurr.x - _movePrev.x);

            moveDirection = (objectUpDirection + objectSidewaysDirection);

            axis = Vector3.Cross(moveDirection, _eye).normalized;

            angle *= rotateSpeed;
            quaternion = Quaternion.AngleAxis(angle, axis);

            _eye = quaternion * _eye;
            _up = quaternion * _up;

            _lastAxis = axis;
            _lastAngle = angle;

        }
        else if (!staticMoving && _lastAngle != 0f)
        {

            _lastAngle *= Mathf.Sqrt(1.0f - dynamicDampingFactor);
            _eye = transform.position - target.position;
            quaternion = Quaternion.AngleAxis(_lastAngle, _lastAxis);
            _eye = quaternion * _eye;
            _up = quaternion * _up;
        }

        _movePrev = _moveCurr;

    }


    private void zoomCamera()
    {

        // if (_state == STATE.TOUCH_ZOOM_PAN)
        // {

        //     factor = _touchZoomDistanceStart / _touchZoomDistanceEnd;
        //     _touchZoomDistanceStart = _touchZoomDistanceEnd;
        //     _eye.multiplyScalar(factor);

        // }
        // else

        float factor = 1f + (_zoomEnd.y - _zoomStart.y) * zoomSpeed;

        target.transform.localScale = target.transform.localScale * factor;

        if (factor != 1f && factor > 0f)
        {

            _eye *= (factor);

        }

        if (staticMoving)
        {

            _zoomStart = _zoomEnd;

        }
        else
        {

            _zoomStart.y += (_zoomEnd.y - _zoomStart.y) * this.dynamicDampingFactor;

        }
    }

    private void panCamera()
    {
        var mouseChange = new Vector2();
        var pan = new Vector3();

        mouseChange = _panEnd - _panStart;

        if (mouseChange.sqrMagnitude > EPS)
        {
            mouseChange *= _eye.magnitude * panSpeed;
            pan = Vector3.Cross(_eye, transform.up).normalized * mouseChange.x;
            pan += transform.up * mouseChange.y;

            target.position -= pan;
            transform.position -= pan;

            if (staticMoving)
            {
                _panStart = _panEnd;
            }
            else
            {
                _panStart += (_panEnd - _panStart) * (dynamicDampingFactor);
            }
        }
    }

    private void checkDistances()
    {
        if (!noZoom || !noPan)
        {
            if (_eye.magnitude > maxDistance * maxDistance)
            {
                transform.position = target.position + _eye.normalized * maxDistance;
                _zoomStart = _zoomEnd;
            }

            if (_eye.magnitude < minDistance * minDistance)
            {

                transform.position = target.position + _eye.normalized * minDistance;
                _zoomStart = _zoomEnd;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            if (Input.GetMouseButtonDown(i))
                mousedown(i);

            if (Input.GetMouseButton(i))
                mousemove(i);

            if (Input.GetMouseButtonUp(i))
                mouseup(i);

            if (Input.mouseScrollDelta.y != 0)
                mousewheel();
        }

        _eye = transform.position - target.position;

        if (!noRotate)
        {
            rotateCamera();
        }

        if (!noZoom)
        {
            zoomCamera();
        }

        if (!noPan)
        {
            panCamera();
        }


        if (Input.GetKey(KeyCode.W))
        {
            target.position = target.position + transform.forward * m_flySensitivity;
        }

        if (Input.GetKey(KeyCode.S))
        {
            target.position = target.position - transform.forward * m_flySensitivity;
        }

        if (Input.GetKey(KeyCode.A))
        {
            target.position = target.position - transform.right * m_flySensitivity;
        }

        if (Input.GetKey(KeyCode.D))
        {
            target.position = target.position + transform.right * m_flySensitivity;
        }


        transform.position = target.position + _eye;
        checkDistances();
        this.transform.LookAt(target.position);

        if ((lastPosition - transform.position).sqrMagnitude > EPS)
        {
            lastPosition = this.transform.position;
        }
    }

    private void Start()
    {
        _state = STATE.NONE;
        _prevState = STATE.NONE;
        position0 = transform.position;
        up0 = _up;
        _eye = transform.position - target.position;
    }

    private void reset()
    {
        _state = STATE.NONE;
        _prevState = STATE.NONE;
        transform.position = position0;

        _up = up0;
        _eye = transform.position - target.position;

        transform.LookAt(target);
        lastPosition = transform.position;
    }
    private void mousedown(int button)
    {

        if (_state == STATE.NONE)
        {
            _state = (STATE)button;
        }

        if (_state == STATE.ROTATE && !noRotate)
        {
            _moveCurr = getMouseOnCircle((int)Input.mousePosition.x, (int)Input.mousePosition.y);
            _movePrev = _moveCurr;
            // target.gameObject.GetComponentInChildren<Renderer>().enabled = true;
        }
        else if (_state == STATE.ZOOM && !noZoom)
        {
            _zoomStart = normalizedMouseCoord((int)Input.mousePosition.x, (int)Input.mousePosition.y);
            _zoomEnd = _zoomStart;
        }
        else if (_state == STATE.PAN && !noPan)
        {
            _panStart = normalizedMouseCoord((int)Input.mousePosition.x, (int)Input.mousePosition.y);
            _panEnd = _panStart;
        }
    }

    private void mousemove(int button)
    {

        if (_state == STATE.ROTATE && !noRotate)
        {
            _movePrev = (_moveCurr);
            _moveCurr = (getMouseOnCircle((int)Input.mousePosition.x, (int)Input.mousePosition.y));
        }
        else if (_state == STATE.ZOOM && !noZoom)
        {
            _zoomEnd = (normalizedMouseCoord((int)Input.mousePosition.x, (int)Input.mousePosition.y));
        }
        else if (_state == STATE.PAN && !noPan)
        {
            _panEnd = (normalizedMouseCoord((int)Input.mousePosition.x, (int)Input.mousePosition.y));
        }
    }

    private void mouseup(int button)
    {
        _state = STATE.NONE;
        // target.gameObject.GetComponentInChildren<Renderer>().enabled = false;
    }

    private void mousewheel()
    {
        _zoomStart.y -= Input.GetAxis("Mouse ScrollWheel") * 0.00025f;
        Debug.Log(_zoomStart.y);
    }

    //     private void touchstart( event )
    // {

    //         if (enabled == false) return;

    //         switch ( event.touches.length ) {

    // 			case 1:
    // 				_state = STATE.TOUCH_ROTATE;
    //         _moveCurr.copy(getMouseOnCircle( event.touches [0].pageX, event.touches [0].pageY));
    //     _movePrev.copy(_moveCurr);
    //         break;

    //         default: // 2 or more
    // 				_state = STATE.TOUCH_ZOOM_PAN;
    //         var dx = event.touches [0].pageX - event.touches [1].pageX;
    //     var dy = event.touches [0].pageY - event.touches [1].pageY;
    //     _touchZoomDistanceEnd = _touchZoomDistanceStart = Math.sqrt(dx* dx + dy* dy);

    //     var x = ( event.touches [0].pageX + event.touches [1].pageX ) / 2;
    //     var y = ( event.touches [0].pageY + event.touches [1].pageY ) / 2;
    //     _panStart.copy(normalizedMouseCoord(x, y));
    //         _panEnd.copy(_panStart);
    //         break;

    //     }

    //     dispatchEvent(startEvent);

    // }

    // private void touchmove( event )
    // {

    //     if (enabled == false) return;

    // 		event.preventDefault();
    // 		event.stopPropagation();

    //     switch ( event.touches.length ) {

    // 			case 1:
    // 				_movePrev.copy(_moveCurr);
    //         _moveCurr.copy(getMouseOnCircle( event.touches[0].pageX, event.touches[0].pageY));
    //         break;

    //         default: // 2 or more
    // 				var dx = event.touches[0].pageX - event.touches[1].pageX;
    //         var dy = event.touches[0].pageY - event.touches[1].pageY;
    //         _touchZoomDistanceEnd = Math.sqrt(dx * dx + dy * dy);

    //         var x = ( event.touches[0].pageX + event.touches[1].pageX ) / 2;
    //         var y = ( event.touches[0].pageY + event.touches[1].pageY ) / 2;
    //         _panEnd.copy(normalizedMouseCoord(x, y));
    //         break;

    //     }

    // }

    // private void touchend( event )
    // {

    //     if (enabled == false) return;

    //     switch ( event.touches.length ) {

    // 			case 0:
    // 				_state = STATE.NONE;
    //         break;

    // 			case 1:
    // 				_state = STATE.TOUCH_ROTATE;
    //         _moveCurr.copy(getMouseOnCircle( event.touches[0].pageX, event.touches[0].pageY));
    //         _movePrev.copy(_moveCurr);
    //         break;

    //     }

    //     dispatchEvent(endEvent);

    // }

    // private void contextmenu( event )
    // {

    //     if (enabled == false) return;

    // 		event.preventDefault();

    // }

}